using Gateway.Domain.ProxyConfig;

namespace Gateway.Domain.Abstractions;

public interface IProxyConfigStore {
    /// <summary>
    /// Retorna la configuración activa (si existe).
    /// Debe incluir Routes + Transforms + Clusters + Destinations.
    /// </summary>
    Task<ProxyConfigRoot?> GetActiveAsync(CancellationToken ct);

    /// <summary>
    /// Retorna solo la versión activa (rápido) para detectar cambios.
    /// </summary>
    Task<long?> GetActiveVersionAsync(CancellationToken ct);
}
