using Gateway.Infrastructure.Database.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Gateway.Infrastructure.Database.Context;

public sealed class GatewayDbContextFactory : IDesignTimeDbContextFactory<GatewayDbContext> {
    public GatewayDbContext CreateDbContext(string[] args) {
        // Usa appsettings.json del proyecto de inicio (Gateway.Api) si lo ejecutas con -s Gateway.Api.
        // Si no, igual intenta cargar un appsettings local.
        var basePath = Directory.GetCurrentDirectory();

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var dbOptions = config.GetSection("Database").Get<DatabaseOptions>() ?? new DatabaseOptions();
        var provider = (dbOptions.Provider ?? "Sqlite").Trim().ToLowerInvariant();

        var optionsBuilder = new DbContextOptionsBuilder<GatewayDbContext>();

        switch (provider) {
            case "postgres":
            case "postgressql":
            case "postgresql":
                if (string.IsNullOrWhiteSpace(dbOptions.Postgres.ConnectionString))
                    throw new InvalidOperationException("Database:Postgres:ConnectionString is missing.");
                optionsBuilder.UseNpgsql(dbOptions.Postgres.ConnectionString);
                break;

            case "sqlite":
                if (string.IsNullOrWhiteSpace(dbOptions.Sqlite.ConnectionString))
                    throw new InvalidOperationException("Database:Sqlite:ConnectionString is missing.");
                optionsBuilder.UseSqlite(dbOptions.Sqlite.ConnectionString);
                break;

            default:
                throw new InvalidOperationException($"Unsupported Database:Provider '{dbOptions.Provider}'. Use 'Postgres' or 'Sqlite'.");
        }

        return new GatewayDbContext(optionsBuilder.Options);
    }
}
