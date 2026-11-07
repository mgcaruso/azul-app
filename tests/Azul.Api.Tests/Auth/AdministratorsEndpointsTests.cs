using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Azul.Api.DTOs.Administrators;
using Azul.Api.DTOs.Auth;

namespace Azul.Api.Tests.Auth;

// "admin" signs in as admin@test.com, the bootstrap admin set in AuthApiFactory. That account is shared by
// the whole collection and these tests never remove it (removing yourself is rejected).
[Collection(ApiCollection.Name)]
public class AdministratorsEndpointsTests(AuthApiFactory factory)
{
    private const string AdministratorsUrl = "/api/admin/administrators";
    private const string PanelUrl = "/api/admin/providers";

    [Fact]
    public async Task Without_session_returns_401()
    {
        var response = await factory.CreateClient().GetAsync(AdministratorsUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Regular_user_gets_403_on_every_endpoint()
    {
        var client = factory.CreateClient();
        var me = await SignInAsync(client, NewSubject());

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync(AdministratorsUrl)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await AddAsync(client, "someone@test.com")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteAsync($"{AdministratorsUrl}/{me.Id}")).StatusCode);
    }

    [Fact]
    public async Task GetAll_lists_the_bootstrap_admin()
    {
        var admin = factory.CreateClient();
        await SignInAsync(admin, "admin");

        var response = await admin.GetAsync(AdministratorsUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var administrators = await response.Content.ReadFromJsonAsync<List<AdministratorDto>>();
        Assert.Contains(administrators!, a => a.Email == "admin@test.com");
    }

    [Fact]
    public async Task Add_makes_an_existing_user_admin()
    {
        var subject = NewSubject();
        await SignInAsync(factory.CreateClient(), subject);
        var admin = await SignInAdminAsync();

        var add = await AddAsync(admin, $"{subject}@test.com");

        Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);
        var listed = await (await admin.GetAsync(AdministratorsUrl)).Content.ReadFromJsonAsync<List<AdministratorDto>>();
        Assert.Contains(listed!, a => a.Email == $"{subject}@test.com");
        // The new admin signs in again: the session cookie is built with the roles they have at that moment.
        var newAdmin = factory.CreateClient();
        Assert.True((await SignInAsync(newAdmin, subject)).IsAdmin);
        Assert.Equal(HttpStatusCode.OK, (await newAdmin.GetAsync(PanelUrl)).StatusCode);
    }

    [Fact]
    public async Task Add_is_case_insensitive_on_the_email()
    {
        var subject = NewSubject();
        await SignInAsync(factory.CreateClient(), subject);
        var admin = await SignInAdminAsync();

        var add = await AddAsync(admin, $"{subject}@TEST.com");

        Assert.Equal(HttpStatusCode.NoContent, add.StatusCode);
    }

    [Fact]
    public async Task Add_twice_returns_409()
    {
        var subject = NewSubject();
        await SignInAsync(factory.CreateClient(), subject);
        var admin = await SignInAdminAsync();
        await AddAsync(admin, $"{subject}@test.com");

        var again = await AddAsync(admin, $"{subject}@test.com");

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal("admin.already_admin", await ReadCodeAsync(again));
    }

    [Fact]
    public async Task Add_unknown_email_returns_404()
    {
        var admin = await SignInAdminAsync();

        var response = await AddAsync(admin, $"{NewSubject()}@test.com");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Add_invalid_email_returns_400()
    {
        var admin = await SignInAdminAsync();

        var response = await AddAsync(admin, "not-an-email");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Remove_cuts_the_access_of_that_admin_right_away()
    {
        var subject = NewSubject();
        var other = factory.CreateClient();
        var otherMe = await SignInAsync(other, subject);
        var admin = await SignInAdminAsync();
        await AddAsync(admin, $"{subject}@test.com");
        // Signs in again so their cookie already carries the Admin role.
        await SignInAsync(other, subject);
        Assert.Equal(HttpStatusCode.OK, (await other.GetAsync(PanelUrl)).StatusCode);

        var remove = await admin.DeleteAsync($"{AdministratorsUrl}/{otherMe.Id}");

        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);
        // The security stamp changed: their old session is no longer valid.
        Assert.Equal(HttpStatusCode.Unauthorized, (await other.GetAsync(PanelUrl)).StatusCode);
        var listed = await (await admin.GetAsync(AdministratorsUrl)).Content.ReadFromJsonAsync<List<AdministratorDto>>();
        Assert.DoesNotContain(listed!, a => a.Id == otherMe.Id);
    }

    [Fact]
    public async Task Remove_yourself_returns_409()
    {
        var admin = factory.CreateClient();
        var me = await SignInAsync(admin, "admin");

        var response = await admin.DeleteAsync($"{AdministratorsUrl}/{me.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("admin.cannot_remove_itself", await ReadCodeAsync(response));
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync(PanelUrl)).StatusCode);
    }

    [Fact]
    public async Task Remove_unknown_user_returns_404()
    {
        var admin = await SignInAdminAsync();

        var response = await admin.DeleteAsync($"{AdministratorsUrl}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Remove_user_that_is_not_admin_returns_404()
    {
        var other = await SignInAsync(factory.CreateClient(), NewSubject());
        var admin = await SignInAdminAsync();

        var response = await admin.DeleteAsync($"{AdministratorsUrl}/{other.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static string NewSubject() => Guid.NewGuid().ToString("N");

    private async Task<HttpClient> SignInAdminAsync()
    {
        var client = factory.CreateClient();
        await SignInAsync(client, "admin");
        return client;
    }

    private static async Task<MeDto> SignInAsync(HttpClient client, string subject)
    {
        var response = await client.PostAsJsonAsync("/api/auth/google", new GoogleSignInDto(FakeGoogleTokenValidator.TokenFor(subject)));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MeDto>())!;
    }

    private static Task<HttpResponseMessage> AddAsync(HttpClient client, string email) =>
        client.PostAsJsonAsync(AdministratorsUrl, new AdministratorAddDto { Email = email });

    private static async Task<string?> ReadCodeAsync(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }
}
