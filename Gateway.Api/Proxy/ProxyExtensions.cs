// Proxy/ProxyExtensions.cs
using Gateway.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Yarp.ReverseProxy;

namespace Gateway.Api.Proxy;

public static class ProxyExtensions {
    public static IServiceCollection AddGatewayProxy(this IServiceCollection services, IConfiguration config) {
        services
            .AddReverseProxy()
            .LoadFromConfig(config.GetSection("ReverseProxy"));

        return services;
    }

    public static IEndpointRouteBuilder MapGatewayProxyEndpoints(this IEndpointRouteBuilder app) {
        // Aplica autorización a todas las rutas YARP que tengan AuthorizationPolicy definida (en reverseproxy.json).
        app.MapReverseProxy();

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
           .AllowAnonymous();

        return app;
    }
}
