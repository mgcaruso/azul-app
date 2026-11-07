using System.Net;
using System.Net.Http.Json;
using Azul.Api.DTOs.Auth;

namespace Azul.Api.Tests.Auth;

// The admin panel is only for users with the Admin role. "admin@test.com" is the bootstrap admin set in AuthApiFactory
// (the fake Google validator builds the email as "{subject}@test.com", so subject "admin" is the admin).
[Collection(ApiCollection.Name)]
public class AdminAccessTests(AuthApiFactory factory)
{
    private const string AdminPanelUrl = "/api/admin/providers";

    [Fact]
    public async Task Bootstrap_admin_sign_in_returns_is_admin_and_can_open_the_panel()
    {
        var client = factory.CreateClient();

        var signIn = await SignInAsync(client, "admin");

        var me = await signIn.Content.ReadFromJsonAsync<MeDto>();
        Assert.True(me!.IsAdmin);
        // Proves the role is assigned BEFORE the session cookie is built: the very first session already has it.
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(AdminPanelUrl)).StatusCode);
        Assert.True((await (await client.GetAsync("/api/me")).Content.ReadFromJsonAsync<MeDto>())!.IsAdmin);
    }

    [Fact]
    public async Task Regular_user_is_not_admin_and_gets_403_on_the_panel()
    {
        var client = factory.CreateClient();

        var signIn = await SignInAsync(client, Guid.NewGuid().ToString("N"));

        var me = await signIn.Content.ReadFromJsonAsync<MeDto>();
        Assert.False(me!.IsAdmin);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync(AdminPanelUrl)).StatusCode);
    }

    [Fact]
    public async Task Panel_without_session_returns_401()
    {
        var response = await factory.CreateClient().GetAsync(AdminPanelUrl);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Bootstrap_admin_signing_in_again_keeps_the_role()
    {
        // Second sign-in takes the "already has the role" path: it must not fail or duplicate the assignment.
        await SignInAsync(factory.CreateClient(), "admin");
        var client = factory.CreateClient();

        var signIn = await SignInAsync(client, "admin");

        Assert.True((await signIn.Content.ReadFromJsonAsync<MeDto>())!.IsAdmin);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(AdminPanelUrl)).StatusCode);
    }

    private static Task<HttpResponseMessage> SignInAsync(HttpClient client, string subject) =>
        client.PostAsJsonAsync("/api/auth/google", new GoogleSignInDto(FakeGoogleTokenValidator.TokenFor(subject)));
}
