using Gateway.Domain.Abstractions;
using Gateway.Domain.ProxyConfig;
using Gateway.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.ProxyConfig;

public sealed class EfProxyConfigStore : IProxyConfigStore {
    private readonly GatewayDbContext _db;

    public EfProxyConfigStore(GatewayDbContext db) => _db = db;

    public async Task<ProxyConfigRoot?> GetActiveAsync(CancellationToken ct) {
        return await _db.ProxyConfigs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Version)
            .Include(x => x.Routes)
                .ThenInclude(r => r.Transforms)
            .Include(x => x.Clusters)
                .ThenInclude(c => c.Destinations)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<long?> GetActiveVersionAsync(CancellationToken ct) {
        return await _db.ProxyConfigs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Version)
            .Select(x => (long?)x.Version)
            .FirstOrDefaultAsync(ct);
    }
}