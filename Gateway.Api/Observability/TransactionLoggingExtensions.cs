// Gateway.Api/Observability/TransactionLoggingExtensions.cs
namespace Gateway.Api.Observability;

public static class TransactionLoggingExtensions {
    public static IServiceCollection AddGatewayTransactionLogging(this IServiceCollection services, IConfiguration config) {
        services.Configure<TransactionLoggingOptions>(config.GetSection("TransactionLogging"));
        services.AddScoped<TransactionLoggingMiddleware>();
        return services;
    }

    public static IApplicationBuilder UseGatewayTransactionLogging(this IApplicationBuilder app) {
        // IMiddleware -> UseMiddleware<>() funciona perfecto
        app.UseMiddleware<TransactionLoggingMiddleware>();
        return app;
    }
}
