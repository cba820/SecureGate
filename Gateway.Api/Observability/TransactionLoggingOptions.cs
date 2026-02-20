// Gateway.Api/Observability/TransactionLoggingOptions.cs
namespace Gateway.Api.Observability;

public sealed class TransactionLoggingOptions {
    public bool Enabled { get; set; } = true;

    // Headers que sí vale la pena guardar
    public string[] AllowedHeaders { get; set; } = new[]
    {
        "x-correlation-id",
        "x-request-id",
        "user-agent"
    };

    // Query keys a enmascarar (por defecto: ninguno)
    public string[] MaskQueryKeys { get; set; } = new[] { "password" };
}
