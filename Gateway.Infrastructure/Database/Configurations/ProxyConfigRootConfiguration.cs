using Gateway.Domain.ProxyConfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gateway.Infrastructure.Database.Configurations {
    public sealed class ProxyConfigRootConfiguration : IEntityTypeConfiguration<ProxyConfigRoot> {
        public void Configure(EntityTypeBuilder<ProxyConfigRoot> b) {
            b.ToTable("ProxyConfigs");
            b.HasKey(x => x.Id);

            b.Property(x => x.Version).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();

            b.HasMany(x => x.Routes)
                .WithOne()
                .HasForeignKey("ProxyConfigId")
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.Clusters)
                 .WithOne()
                 .HasForeignKey(x => x.ProxyConfigId)
                 .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
