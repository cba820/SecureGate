using Gateway.Domain.Auditing;
using Gateway.Domain.ProxyConfig;
using Gateway.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Database.Context;

public sealed class GatewayDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid> {
    public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options) { }

    public DbSet<GatewayTransactionLog> GatewayTransactionLogs => Set<GatewayTransactionLog>();
    public DbSet<ProxyConfigRoot> ProxyConfigs => Set<ProxyConfigRoot>();
    public DbSet<ProxyRoute> ProxyRoutes => Set<ProxyRoute>();
    public DbSet<ProxyCluster> ProxyClusters => Set<ProxyCluster>();
    public DbSet<ProxyDestination> ProxyDestinations => Set<ProxyDestination>();
    public DbSet<RouteTransform> RouteTransforms => Set<RouteTransform>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GatewayDbContext).Assembly);
    }
}
