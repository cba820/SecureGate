using Gateway.Domain.ProxyConfig;
using Gateway.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gateway.Infrastructure.ProxyConfig {
    public static class ProxyConfigSeeder {
        public static async Task SeedDefaultProxyConfigAsync(
            IServiceProvider services,
            ILogger logger,
            CancellationToken ct = default) {
            try {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();

                // Si ya existe alguna config activa, no hacemos nada
                var hasAny = await db.ProxyConfigs.AsNoTracking().AnyAsync(ct);
                if (hasAny) {
                    logger.LogInformation("Proxy config seeding skipped (ProxyConfigs table is not empty).");
                    return;
                }

                logger.LogInformation("Seeding default proxy config (apicertificado)...");

                var root = new ProxyConfigRoot(version: 1, isActive: true);

                var route = new ProxyRoute(
                    routeId: "apicertificado_all",
                    clusterId: "apicertificado_cluster",
                    pathMatch: "/apicertificado/{**catch-all}",
                    authorizationPolicy: "ProxyPolicy"
                );

                route.AddTransform(new RouteTransform("PathPattern", "/{**catch-all}"));

                var cluster = new ProxyCluster("apicertificado_cluster");
                cluster.AddDestination(new ProxyDestination("d1", "http://172.20.1.21:9533/"));

                root.AddRoute(route);
                root.AddCluster(cluster);

                db.ProxyConfigs.Add(root);
                await db.SaveChangesAsync(ct);

                logger.LogInformation("Default proxy config seeded successfully. Version={Version}", root.Version);
            }
            catch (Exception ex) {
                logger.LogError(ex, "Proxy config seeding failed. The API will continue to start, but routing may be empty.");
            }
        }
    }
}
