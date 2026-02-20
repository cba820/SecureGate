using Gateway.Domain.Auditing;
using Gateway.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Database.Context;

public sealed class GatewayDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid> {
    public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options) { }

    public DbSet<GatewayTransactionLog> GatewayTransactionLogs => Set<GatewayTransactionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GatewayDbContext).Assembly);
    }
}
