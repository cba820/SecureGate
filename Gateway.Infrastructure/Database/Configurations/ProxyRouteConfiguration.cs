using Gateway.Domain.ProxyConfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gateway.Infrastructure.Database.Configurations {
    public sealed class ProxyRouteConfiguration : IEntityTypeConfiguration<ProxyRoute> {
        public void Configure(EntityTypeBuilder<ProxyRoute> b) {
            b.ToTable("ProxyRoutes");
            b.HasKey(x => x.Id);

            b.Property(x => x.RouteId).IsRequired().HasMaxLength(200);
            b.Property(x => x.ClusterId).IsRequired().HasMaxLength(200);
            b.Property(x => x.PathMatch).IsRequired().HasMaxLength(500);
            b.Property(x => x.AuthorizationPolicy).HasMaxLength(200);
            b.Property(x => x.IsActive).IsRequired();

            b.HasMany(x => x.Transforms)
                .WithOne()
                .HasForeignKey("ProxyRouteId")
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex("ProxyConfigId");
            b.HasIndex(x => x.RouteId).IsUnique(false);
        }
    }
}
