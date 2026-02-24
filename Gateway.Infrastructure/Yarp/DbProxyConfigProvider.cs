using Gateway.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace Gateway.Infrastructure.Yarp;

public sealed class DbProxyConfigProvider : BackgroundService, IProxyConfigProvider {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DbProxyConfigProvider> _logger;
    private readonly DbProxyConfigProviderOptions _options;

    private volatile YarpProxyConfig _current = YarpProxyConfig.Empty();

    public DbProxyConfigProvider(
        IServiceScopeFactory scopeFactory,
        IOptions<DbProxyConfigProviderOptions> options,
        ILogger<DbProxyConfigProvider> logger) {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    public IProxyConfig GetConfig() => _current;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        await TryReloadAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested) {
            try {
                await Task.Delay(_options.PollInterval, stoppingToken);
                await TryReloadIfVersionChangedAsync(stoppingToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) {
                _logger.LogError(ex, "Error in DB proxy config polling loop.");
            }
        }
    }

    private async Task TryReloadIfVersionChangedAsync(CancellationToken ct) {
        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IProxyConfigStore>();

        var latestVersion = await store.GetActiveVersionAsync(ct);

        if (latestVersion is null) {
            _logger.LogWarning("No active proxy config found in DB (version check).");
            return;
        }

        if (latestVersion.Value == _current.Version)
            return;

        await TryReloadAsync(ct);
    }

    private async Task TryReloadAsync(CancellationToken ct) {
        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IProxyConfigStore>();

        var root = await store.GetActiveAsync(ct);
        if (root is null) {
            _logger.LogWarning("No active proxy config found in DB (full load).");
            return;
        }

        var (routes, clusters) = YarpConfigMapper.Map(root);

        if (routes.Count == 0 || clusters.Count == 0) {
            _logger.LogWarning("Loaded proxy config v{Version} but routes={Routes} clusters={Clusters}.",
                root.Version, routes.Count, clusters.Count);
        }

        var next = new YarpProxyConfig(root.Version, routes, clusters);

        var prev = _current;
        _current = next;
        prev.SignalChange();

        _logger.LogInformation("Proxy config reloaded from DB. Version={Version}, Routes={Routes}, Clusters={Clusters}",
            root.Version, routes.Count, clusters.Count);
    }

    private sealed class YarpProxyConfig : IProxyConfig {
        private readonly CancellationTokenSource _cts = new();

        public long Version { get; }
        public IReadOnlyList<RouteConfig> Routes { get; }
        public IReadOnlyList<ClusterConfig> Clusters { get; }
        public IChangeToken ChangeToken { get; }

        public YarpProxyConfig(long version, IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters) {
            Version = version;
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(_cts.Token);
        }

        public void SignalChange() {
            if (!_cts.IsCancellationRequested)
                _cts.Cancel();
        }

        public static YarpProxyConfig Empty() =>
            new YarpProxyConfig(0, Array.Empty<RouteConfig>(), Array.Empty<ClusterConfig>());
    }
}