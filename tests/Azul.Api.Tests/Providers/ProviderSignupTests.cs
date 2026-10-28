using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Azul.Api.Data;
using Azul.Api.DTOs.Auth;
using Azul.Api.Entities;
using Azul.Api.Tests.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Azul.Api.Tests.Providers;

[Collection(ApiCollection.Name)]
public class ProviderSignupTests(AuthApiFactory factory)
{
    [Fact]
    public async Task Alta_sin_sesion_devuelve_401()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/providers", NewProvider());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Alta_con_sesion_crea_la_ficha_del_usuario_en_revision()
    {
        var client = await SignedInClientAsync();
        var me = await client.GetFromJsonAsync<MeDto>("/api/me");

        var response = await client.PostAsJsonAsync("/api/providers", NewProvider());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var providerId = json.RootElement.GetProperty("id").GetInt32();

        // El UserId no viaja en la respuesta: se mira en la base que la ficha quedó atada a quien la creó.
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var provider = await dbContext.Providers.SingleAsync(p => p.Id == providerId);
        Assert.Equal(me!.Id, provider.UserId);
        Assert.Equal(ProviderStatus.Pending, provider.Status);
    }

    [Fact]
    public async Task Segunda_alta_del_mismo_usuario_devuelve_409()
    {
        var client = await SignedInClientAsync();
        await client.PostAsJsonAsync("/api/providers", NewProvider());

        var second = await client.PostAsJsonAsync("/api/providers", NewProvider());

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("provider.already_exists", await ReadCodeAsync(second));
    }

    [Fact]
    public async Task Dos_usuarios_distintos_tienen_cada_uno_su_ficha()
    {
        var ana = await SignedInClientAsync();
        var beto = await SignedInClientAsync();

        var first = await ana.PostAsJsonAsync("/api/providers", NewProvider());
        var second = await beto.PostAsJsonAsync("/api/providers", NewProvider());

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
    }

    [Fact]
    public async Task Me_dice_si_el_usuario_ya_tiene_ficha()
    {
        var client = await SignedInClientAsync();

        var before = await client.GetFromJsonAsync<MeDto>("/api/me");
        await client.PostAsJsonAsync("/api/providers", NewProvider());
        var after = await client.GetFromJsonAsync<MeDto>("/api/me");

        Assert.False(before!.IsProvider);
        Assert.True(after!.IsProvider);
    }

    [Fact]
    public async Task Ingresar_con_ficha_ya_creada_devuelve_isProvider_true()
    {
        var subject = Guid.NewGuid().ToString("N");
        var client = await SignedInClientAsync(subject);
        await client.PostAsJsonAsync("/api/providers", NewProvider());

        // Otro dispositivo de la misma persona: la respuesta del login ya trae isProvider.
        var signIn = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/google", new GoogleSignInDto(FakeGoogleTokenValidator.TokenFor(subject)));
        var me = await signIn.Content.ReadFromJsonAsync<MeDto>();

        Assert.True(me!.IsProvider);
    }

    [Fact]
    public async Task Mi_ficha_devuelve_la_ficha_propia_con_su_estado()
    {
        var client = await SignedInClientAsync();
        await client.PostAsJsonAsync("/api/providers", NewProvider());

        var response = await client.GetAsync("/api/me/provider");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Pending", json.RootElement.GetProperty("status").GetString());
        Assert.Equal("Ana Test", json.RootElement.GetProperty("displayName").GetString());
    }

    [Fact]
    public async Task Mi_ficha_no_muestra_la_de_otra_persona()
    {
        var ana = await SignedInClientAsync();
        await ana.PostAsJsonAsync("/api/providers", NewProvider());
        var beto = await SignedInClientAsync();

        var response = await beto.GetAsync("/api/me/provider");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Mi_ficha_sin_sesion_devuelve_401()
    {
        var response = await factory.CreateClient().GetAsync("/api/me/provider");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Un cliente con la cookie de un usuario nuevo (cada test usa el suyo, así no se pisan las fichas).
    private async Task<HttpClient> SignedInClientAsync(string? subject = null)
    {
        var client = factory.CreateClient();
        var token = FakeGoogleTokenValidator.TokenFor(subject ?? Guid.NewGuid().ToString("N"));
        var signIn = await client.PostAsJsonAsync("/api/auth/google", new GoogleSignInDto(token));
        signIn.EnsureSuccessStatusCode();
        return client;
    }

    // Celular distinto en cada alta: hay un tope de fichas por número.
    private static object NewProvider() => new
    {
        type = "Individual",
        firstName = "Ana",
        lastName = "Test",
        phoneNumber = $"+549228{Random.Shared.Next(1_000_000, 9_999_999)}",
        offeringIds = new[] { 1 },
        acceptedTerms = true
    };

    private static async Task<string?> ReadCodeAsync(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }
}
