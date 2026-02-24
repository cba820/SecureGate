using Gateway.Domain.ProxyConfig;
using Yarp.ReverseProxy.Configuration;

namespace Gateway.Infrastructure.Yarp;

public static class YarpConfigMapper {
    public static (IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters) Map(ProxyConfigRoot root) {
        var routes = root.Routes
            .Where(r => r.IsActive)
            .Select(r => new RouteConfig {
                RouteId = r.RouteId,
                ClusterId = r.ClusterId,
                AuthorizationPolicy = r.AuthorizationPolicy,
                Match = new RouteMatch {
                    Path = r.PathMatch
                },
                Transforms = r.Transforms.Count == 0
                    ? null
                    : r.Transforms
                        .Select(t => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                            [t.Key] = t.Value
                        })
                        .ToList()
            })
            .ToList();

        var clusters = root.Clusters
            .Where(c => c.IsActive)
            .Select(c => {
                var destinations = c.Destinations
                    .Where(d => d.IsActive)
                    .ToDictionary(
                        d => d.DestinationId,
                        d => new DestinationConfig {
                            Address = d.Address
                        },
                        StringComparer.OrdinalIgnoreCase);

                return new ClusterConfig {
                    ClusterId = c.ClusterId,
                    Destinations = destinations
                };
            })
            .ToList();

        return (routes, clusters);
    }
}