using Gateway.Domain.ProxyConfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gateway.Infrastructure.Database.Configurations {
    public sealed class ProxyClusterConfiguration : IEntityTypeConfiguration<ProxyCluster> {
        public void Configure(EntityTypeBuilder<ProxyCluster> b) {
            b.ToTable("ProxyClusters");
            b.HasKey(x => x.Id);

            b.Property(x => x.ClusterId).IsRequired().HasMaxLength(200);
            b.Property(x => x.IsActive).IsRequired();

            b.HasMany(x => x.Destinations)
                .WithOne()
                .HasForeignKey("ProxyClusterId")
                .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.ProxyConfigId).IsRequired();
            b.HasIndex(x => x.ProxyConfigId);

            b.HasIndex("ProxyConfigId");
            b.HasIndex(x => x.ClusterId).IsUnique(false);
        }
    }
}
