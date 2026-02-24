using Gateway.Domain.ProxyConfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gateway.Infrastructure.Database.Configurations {
    public sealed class RouteTransformConfiguration : IEntityTypeConfiguration<RouteTransform> {
        public void Configure(EntityTypeBuilder<RouteTransform> b) {
            b.ToTable("RouteTransforms");
            b.HasKey(x => x.Id);

            b.Property(x => x.Key).IsRequired().HasMaxLength(100);
            b.Property(x => x.Value).IsRequired().HasMaxLength(2000);

            b.HasIndex("ProxyRouteId");
        }
    }
}
