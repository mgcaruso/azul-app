using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Azul.Api.DTOs.Auth;

namespace Azul.Api.Tests.Auth;

[Collection(ApiCollection.Name)]
public class AuthEndpointsTests(AuthApiFactory factory)
{
    [Fact]
    public async Task SignIn_con_token_valido_devuelve_el_usuario_y_abre_la_sesion()
    {
        var client = factory.CreateClient();
        var subject = NewSubject();

        var signIn = await SignInAsync(client, subject);

        Assert.Equal(HttpStatusCode.OK, signIn.StatusCode);
        var me = await signIn.Content.ReadFromJsonAsync<MeDto>();
        Assert.Equal($"{subject}@test.com", me!.Email);
        Assert.Equal("Ana Test", me.DisplayName);
        Assert.Contains(signIn.Headers.GetValues("Set-Cookie"), c => c.StartsWith("azul.session=") && c.Contains("httponly"));

        var meResponse = await client.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.Equal(me.Id, (await meResponse.Content.ReadFromJsonAsync<MeDto>())!.Id);
    }

    [Fact]
    public async Task SignIn_de_nuevo_reusa_el_usuario_y_actualiza_el_nombre()
    {
        var subject = NewSubject();

        var first = await (await SignInAsync(factory.CreateClient(), subject, "Ana")).Content.ReadFromJsonAsync<MeDto>();
        var second = await (await SignInAsync(factory.CreateClient(), subject, "Ana María")).Content.ReadFromJsonAsync<MeDto>();

        Assert.Equal(first!.Id, second!.Id);
        Assert.Equal("Ana María", second.DisplayName);
    }

    [Fact]
    public async Task SignIn_con_token_invalido_devuelve_401()
    {
        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/auth/google", new GoogleSignInDto("cualquier-cosa"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("auth.invalid_google_token", await ReadCodeAsync(response));
    }

    [Fact]
    public async Task SignIn_sin_token_devuelve_400()
    {
        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/auth/google", new GoogleSignInDto(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Me_sin_sesion_devuelve_401()
    {
        var response = await factory.CreateClient().GetAsync("/api/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_cierra_la_sesion()
    {
        var client = factory.CreateClient();
        await SignInAsync(client, NewSubject());

        var logout = await client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/me")).StatusCode);
    }

    [Fact]
    public async Task LogoutAll_cierra_la_sesion_en_todos_los_dispositivos()
    {
        var subject = NewSubject();
        var compu = factory.CreateClient();
        var celular = factory.CreateClient();
        await SignInAsync(compu, subject);
        await SignInAsync(celular, subject);

        var logoutAll = await compu.PostAsync("/api/auth/logout-all", null);

        Assert.Equal(HttpStatusCode.NoContent, logoutAll.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await compu.GetAsync("/api/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await celular.GetAsync("/api/me")).StatusCode);
    }

    [Fact]
    public async Task LogoutAll_sin_sesion_devuelve_401()
    {
        var response = await factory.CreateClient().PostAsync("/api/auth/logout-all", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static string NewSubject() => Guid.NewGuid().ToString("N");

    private static Task<HttpResponseMessage> SignInAsync(HttpClient client, string subject, string name = "Ana Test") =>
        client.PostAsJsonAsync("/api/auth/google", new GoogleSignInDto(FakeGoogleTokenValidator.TokenFor(subject, name)));

    private static async Task<string?> ReadCodeAsync(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }
}
