using Ardalis.Specification;
using Gateway.Domain.Abstractions;
using Gateway.Infrastructure.Database.Context;
using Gateway.Infrastructure.Database.Options;
using Gateway.Infrastructure.Database.Repositories;
using Gateway.Infrastructure.Identity;
using Gateway.Infrastructure.ProxyConfig;
using Gateway.Infrastructure.Yarp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;

namespace Gateway.Infrastructure;

public static class DependencyInjection {
    
    public static IServiceCollection AddGatewayInfrastructure(this IServiceCollection services, IConfiguration config) {
        services.AddGatewayDatabase(config);
        services.AddGatewayRepositories();
        services.AddUnitOfWork();
        services.AddReverseProxyConfiguration(config);
        return services;
    }

    public static IServiceCollection AddGatewayDatabase(this IServiceCollection services, IConfiguration config) {
        services.Configure<DatabaseOptions>(config.GetSection("Database").Bind);

        var dbOptions = config.GetSection("Database").Get<DatabaseOptions>()
                        ?? throw new InvalidOperationException("Missing 'Database' configuration section.");

        var provider = (dbOptions.Provider ?? "Sqlite").Trim().ToLowerInvariant();

        services.AddDbContext<GatewayDbContext>(opt =>
        {
            switch (provider) {
                case "postgres":
                case "postgresql":
                case "postgressql": // por si quedó typo en config
                    if (string.IsNullOrWhiteSpace(dbOptions.Postgres.ConnectionString))
                        throw new InvalidOperationException("Database:Postgres:ConnectionString is missing.");

                    opt.UseNpgsql(dbOptions.Postgres.ConnectionString, npgsql =>
                    {
                        npgsql.MigrationsAssembly(typeof(GatewayDbContext).Assembly.FullName);
                    });
                    break;

                case "sqlite":
                    if (string.IsNullOrWhiteSpace(dbOptions.Sqlite.ConnectionString))
                        throw new InvalidOperationException("Database:Sqlite:ConnectionString is missing.");

                    opt.UseSqlite(dbOptions.Sqlite.ConnectionString, sqlite =>
                    {
                        sqlite.MigrationsAssembly(typeof(GatewayDbContext).Assembly.FullName);
                    });
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported Database:Provider '{dbOptions.Provider}'. Use 'Postgres' or 'Sqlite'.");
            }
        });

        services
            .AddIdentityCore<ApplicationUser>(opt => {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireNonAlphanumeric = false;

                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                opt.User.RequireUniqueEmail = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<GatewayDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddUnitOfWork(this IServiceCollection services) {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    public static IServiceCollection AddGatewayRepositories(this IServiceCollection services) {
        // Para inyectar IReadRepositoryBase<T> o IRepositoryBase<T>
        services.AddScoped(typeof(IReadRepositoryBase<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IRepositoryBase<>), typeof(EfRepository<>));

        return services;
    }

    public static IServiceCollection AddReverseProxyConfiguration(this IServiceCollection services, IConfiguration config) {
        services.Configure<DbProxyConfigProviderOptions>(config.GetSection("ProxyConfigProvider"));

        services.AddScoped<IProxyConfigStore, EfProxyConfigStore>();

        services.AddSingleton<DbProxyConfigProvider>();
        services.AddSingleton<IProxyConfigProvider>(sp => sp.GetRequiredService<DbProxyConfigProvider>());
        services.AddHostedService(sp => sp.GetRequiredService<DbProxyConfigProvider>());

        return services;
    }
}