using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gateway.Infrastructure.Database.Context {
    public static class DatabaseMigrator {
        public static async Task MigrateAsync<TDbContext>(
            IServiceProvider services,
            IHostEnvironment env,
            ILogger logger,
            CancellationToken ct = default)
            where TDbContext : DbContext {
            try {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

                // Seguridad: por defecto solo en Development
                if (!env.IsDevelopment()) {
                    logger.LogInformation("Database migration skipped (environment: {Env}).", env.EnvironmentName);
                    return;
                }

                logger.LogInformation("Applying database migrations for {DbContext}...", typeof(TDbContext).Name);
                await db.Database.MigrateAsync(ct);
                logger.LogInformation("Database migrations applied successfully for {DbContext}.", typeof(TDbContext).Name);
            }
            catch (Exception ex) {
                logger.LogError(ex, "Database migration failed. The API will continue to start, but DB may be out of date.");
                // Si prefieres “fail fast” en dev, puedes volver a lanzar:
                // throw;
            }
        }
    }
}
