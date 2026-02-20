// Gateway.Infrastructure/Database/Context/Configurations/GatewayTransactionLogConfiguration.cs
using Gateway.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gateway.Infrastructure.Database.Context.Configurations;

public sealed class GatewayTransactionLogConfiguration : IEntityTypeConfiguration<GatewayTransactionLog> {
    public void Configure(EntityTypeBuilder<GatewayTransactionLog> b) {
        b.ToTable("GatewayTransactionLogs");

        b.HasKey(x => x.Id);

        b.Property(x => x.TimestampUtc).IsRequired();

        b.Property(x => x.TraceId).HasMaxLength(128);
        b.Property(x => x.RequestId).HasMaxLength(128);
        b.Property(x => x.CorrelationId).HasMaxLength(128);

        b.Property(x => x.Username).HasMaxLength(256);
        b.Property(x => x.ClientId).HasMaxLength(128);

        b.Property(x => x.HttpMethod).HasMaxLength(16).IsRequired();
        b.Property(x => x.InboundPath).HasMaxLength(2048).IsRequired();
        b.Property(x => x.InboundQuery).HasMaxLength(4096);

        b.Property(x => x.InboundIp).HasMaxLength(64);
        b.Property(x => x.UserAgent).HasMaxLength(512);

        // JSON string (allowlist)
        b.Property(x => x.InboundHeaders).HasColumnType("TEXT");

        b.Property(x => x.RouteId).HasMaxLength(256);
        b.Property(x => x.ClusterId).HasMaxLength(256);
        b.Property(x => x.DestinationAddress).HasMaxLength(2048);
        b.Property(x => x.ProxiedPath).HasMaxLength(2048);

        b.Property(x => x.ErrorCode).HasMaxLength(128);
        b.Property(x => x.ErrorMessage).HasMaxLength(2048);

        // Índices útiles
        b.HasIndex(x => x.TimestampUtc);
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.StatusCode);
        b.HasIndex(x => x.RouteId);
        b.HasIndex(x => x.ClusterId);
        b.HasIndex(x => x.TraceId);
    }
}
