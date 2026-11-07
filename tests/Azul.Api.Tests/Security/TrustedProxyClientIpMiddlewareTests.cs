using System.Net;
using Azul.Api.Common.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Azul.Api.Tests.Security;

// Tests de unidad del middleware: sin levantar la API ni la base, con un HttpContext armado a mano.
public class TrustedProxyClientIpMiddlewareTests
{
    private const string Secret = "secreto-de-prueba";
    private static readonly IPAddress ProxyIp = IPAddress.Parse("104.28.1.1");

    [Fact]
    public async Task Con_secreto_valido_usa_la_ip_del_header()
    {
        var context = await RunAsync(Secret, secretHeader: Secret, clientIpHeader: "181.10.20.30");

        Assert.Equal(IPAddress.Parse("181.10.20.30"), context.Connection.RemoteIpAddress);
    }

    [Fact]
    public async Task Con_secreto_incorrecto_deja_la_ip_original()
    {
        var context = await RunAsync(Secret, secretHeader: "otro", clientIpHeader: "181.10.20.30");

        Assert.Equal(ProxyIp, context.Connection.RemoteIpAddress);
    }

    [Fact]
    public async Task Sin_secreto_configurado_ignora_los_headers()
    {
        var context = await RunAsync(configuredSecret: null, secretHeader: Secret, clientIpHeader: "181.10.20.30");

        Assert.Equal(ProxyIp, context.Connection.RemoteIpAddress);
    }

    [Fact]
    public async Task Con_ip_invalida_deja_la_ip_original()
    {
        var context = await RunAsync(Secret, secretHeader: Secret, clientIpHeader: "no-es-una-ip");

        Assert.Equal(ProxyIp, context.Connection.RemoteIpAddress);
    }

    [Fact]
    public async Task Saca_el_header_del_secreto()
    {
        var context = await RunAsync(Secret, secretHeader: Secret, clientIpHeader: "181.10.20.30");

        Assert.False(context.Request.Headers.ContainsKey(TrustedProxyClientIpMiddleware.SecretHeader));
    }

    private static async Task<HttpContext> RunAsync(string? configuredSecret, string secretHeader, string clientIpHeader)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Proxy:Secret"] = configuredSecret })
            .Build();

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = ProxyIp;
        context.Request.Headers[TrustedProxyClientIpMiddleware.SecretHeader] = secretHeader;
        context.Request.Headers[TrustedProxyClientIpMiddleware.ClientIpHeader] = clientIpHeader;

        var middleware = new TrustedProxyClientIpMiddleware(_ => Task.CompletedTask, configuration);
        await middleware.InvokeAsync(context);

        return context;
    }
}
