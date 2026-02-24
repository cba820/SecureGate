namespace Gateway.Domain.ProxyConfig;

public sealed class ProxyConfigRoot {
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Incrementa cuando se “publica” una configuración nueva.
    /// Sirve para detectar cambios sin comparar todo.
    /// </summary>
    public long Version { get; private set; } = 1;

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private readonly List<ProxyRoute> _routes = new();
    public IReadOnlyCollection<ProxyRoute> Routes => _routes;

    private readonly List<ProxyCluster> _clusters = new();
    public IReadOnlyCollection<ProxyCluster> Clusters => _clusters;

    private ProxyConfigRoot() { }

    public ProxyConfigRoot(long version, bool isActive = true) {
        Version = version;
        IsActive = isActive;
    }

    public void AddRoute(ProxyRoute route) => _routes.Add(route);
    public void AddCluster(ProxyCluster cluster) => _clusters.Add(cluster);

    public void PublishNewVersion() {
        Version++;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
}