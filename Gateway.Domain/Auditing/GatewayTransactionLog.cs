// Gateway.Domain/Auditing/GatewayTransactionLog.cs
namespace Gateway.Domain.Auditing;

public sealed class GatewayTransactionLog {
    public Guid Id { get; set; } = Guid.NewGuid();

    // Identidad/correlación
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }
    public string? RequestId { get; set; }
    public string? CorrelationId { get; set; }

    // Usuario
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string? ClientId { get; set; }

    // Request
    public string HttpMethod { get; set; } = default!;
    public string InboundPath { get; set; } = default!;
    public string? InboundQuery { get; set; }
    public string? InboundIp { get; set; }
    public string? UserAgent { get; set; }
    public string? InboundHeaders { get; set; } // JSON (allowlist)
    public long? RequestBodySizeBytes { get; set; }

    // Enrutamiento (YARP)
    public string? RouteId { get; set; }
    public string? ClusterId { get; set; }
    public string? DestinationAddress { get; set; }
    public string? ProxiedPath { get; set; }

    // Response
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public long? ResponseBodySizeBytes { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
