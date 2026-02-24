namespace Gateway.Domain.ProxyConfig;

public sealed class ProxyCluster {
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProxyConfigId { get; private set; }

    public string ClusterId { get; private set; } = default!; // ej: apicertificado_cluster
    public bool IsActive { get; private set; } = true;

    private readonly List<ProxyDestination> _destinations = new();
    public IReadOnlyCollection<ProxyDestination> Destinations => _destinations;

    private ProxyCluster() { }

    public ProxyCluster(string clusterId) {
        ClusterId = clusterId;
    }

    public void AddDestination(ProxyDestination destination) => _destinations.Add(destination);
    public void Disable() => IsActive = false;
}