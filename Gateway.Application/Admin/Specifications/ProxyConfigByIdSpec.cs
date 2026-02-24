using Ardalis.Specification;
using Gateway.Domain.ProxyConfig;

namespace Gateway.Application.Admin.Specifications {
    public sealed class ProxyConfigByIdSpec : Specification<ProxyConfigRoot>, ISingleResultSpecification {
        public ProxyConfigByIdSpec(Guid id) {
            Query
                .Where(x => x.Id == id)
                .Include(x => x.Routes)
                    .ThenInclude(r => r.Transforms)
                .Include(x => x.Clusters)
                    .ThenInclude(c => c.Destinations);
        }
    }
}
