using Ardalis.Specification;
using Gateway.Domain.ProxyConfig;

namespace Gateway.Application.Admin.Specifications {
    public sealed class ActiveProxyConfigSpec : Specification<ProxyConfigRoot>, ISingleResultSpecification {
        public ActiveProxyConfigSpec() {
            Query
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.Version)
                .Include(x => x.Routes)
                    .ThenInclude(r => r.Transforms)
                .Include(x => x.Clusters)
                    .ThenInclude(c => c.Destinations);
        }
    }
}
