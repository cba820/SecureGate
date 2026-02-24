namespace Gateway.Domain.ProxyConfig;

public sealed class ProxyRoute {
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string RouteId { get; private set; } = default!;       // ej: apicertificado_all
    public string ClusterId { get; private set; } = default!;     // ej: apicertificado_cluster
    public string PathMatch { get; private set; } = default!;     // ej: /apicertificado/{**catch-all}

    public string? AuthorizationPolicy { get; private set; }      // ej: ProxyPolicy / AdminOnly
    public bool IsActive { get; private set; } = true;

    private readonly List<RouteTransform> _transforms = new();
    public IReadOnlyCollection<RouteTransform> Transforms => _transforms;

    private ProxyRoute() { }

    public ProxyRoute(string routeId, string clusterId, string pathMatch, string? authorizationPolicy = null) {
        RouteId = routeId;
        ClusterId = clusterId;
        PathMatch = pathMatch;
        AuthorizationPolicy = authorizationPolicy;
    }

    public void AddTransform(RouteTransform transform) => _transforms.Add(transform);
    public void Disable() => IsActive = false;
}