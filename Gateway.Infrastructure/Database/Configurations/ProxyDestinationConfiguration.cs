using Gateway.Domain.ProxyConfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gateway.Infrastructure.Database.Configurations {
    public sealed class ProxyDestinationConfiguration : IEntityTypeConfiguration<ProxyDestination> {
        public void Configure(EntityTypeBuilder<ProxyDestination> b) {
            b.ToTable("ProxyDestinations");
            b.HasKey(x => x.Id);

            b.Property(x => x.DestinationId).IsRequired().HasMaxLength(200);
            b.Property(x => x.Address).IsRequired().HasMaxLength(2000);
            b.Property(x => x.IsActive).IsRequired();

            b.HasIndex("ProxyClusterId");
        }
    }
}
