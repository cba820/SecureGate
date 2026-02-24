using Gateway.Api.Admin.Dtos;
using Gateway.Application.Admin.Specifications;
using Gateway.Domain.ProxyConfig;
using Gateway.Infrastructure.Database.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Controllers.Admin;

[ApiController]
[Route("admin/proxy-config/clusters")]
[Authorize(Policy = "AdminOnly")]
public sealed class ProxyClustersAdminController : ControllerBase {
    private readonly IUnitOfWork _uow;

    public ProxyClustersAdminController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<ActionResult> ListClusters(CancellationToken ct) {
        var cfg = await _uow.Repository<ProxyConfigRoot>().FirstOrDefaultAsync(new ActiveProxyConfigSpec(), ct);
        if (cfg is null) return NotFound("No active proxy config found.");

        return Ok(cfg.Clusters.Where(c => c.IsActive));
    }

    [HttpPost]
    public async Task<ActionResult> AddCluster([FromBody] AddClusterRequest req, CancellationToken ct) {
        var cfg = await _uow.Repository<ProxyConfigRoot>().FirstOrDefaultAsync(new ActiveProxyConfigSpec(), ct);
        if (cfg is null) return NotFound("No active proxy config found.");

        if (cfg.Clusters.Any(c => c.ClusterId == req.ClusterId))
            return Conflict($"Cluster '{req.ClusterId}' already exists.");

        cfg.AddCluster(new ProxyCluster(req.ClusterId));
        await _uow.SaveChangesAsync(ct);

        return Ok(new { Message = "Cluster added.", req.ClusterId });
    }

    [HttpDelete("{clusterId}")]
    public async Task<ActionResult> DisableCluster(string clusterId, CancellationToken ct) {
        var cfg = await _uow.Repository<ProxyConfigRoot>().FirstOrDefaultAsync(new ActiveProxyConfigSpec(), ct);
        if (cfg is null) return NotFound("No active proxy config found.");

        var cluster = cfg.Clusters.FirstOrDefault(c => c.ClusterId == clusterId);
        if (cluster is null) return NotFound($"Cluster '{clusterId}' not found.");

        cluster.Disable();
        await _uow.SaveChangesAsync(ct);

        return Ok(new { Message = "Cluster disabled.", clusterId });
    }

    // ------- Destinations -------

    [HttpGet("{clusterId}/destinations")]
    public async Task<ActionResult> ListDestinations(string clusterId, CancellationToken ct) {
        var cfg = await _uow.Repository<ProxyConfigRoot>().FirstOrDefaultAsync(new ActiveProxyConfigSpec(), ct);
        if (cfg is null) return NotFound("No active proxy config found.");

        var cluster = cfg.Clusters.FirstOrDefault(c => c.ClusterId == clusterId && c.IsActive);
        if (cluster is null) return NotFound($"Cluster '{clusterId}' not found or inactive.");

        return Ok(cluster.Destinations.Where(d => d.IsActive));
    }

    [HttpPost("{clusterId}/destinations")]
    public async Task<ActionResult> AddDestination(string clusterId, [FromBody] AddDestinationRequest req, CancellationToken ct) {
        var cfg = await _uow.Repository<ProxyConfigRoot>().FirstOrDefaultAsync(new ActiveProxyConfigSpec(), ct);
        if (cfg is null) return NotFound("No active proxy config found.");

        var cluster = cfg.Clusters.FirstOrDefault(c => c.ClusterId == clusterId && c.IsActive);
        if (cluster is null) return NotFound($"Cluster '{clusterId}' not found or inactive.");

        if (cluster.Destinations.Any(d => d.DestinationId == req.DestinationId))
            return Conflict($"Destination '{req.DestinationId}' already exists in cluster '{clusterId}'.");

        cluster.AddDestination(new ProxyDestination(req.DestinationId, req.Address));
        await _uow.SaveChangesAsync(ct);

        return Ok(new { Message = "Destination added.", clusterId, req.DestinationId, req.Address });
    }

    [HttpDelete("{clusterId}/destinations/{destinationId}")]
    public async Task<ActionResult> DisableDestination(string clusterId, string destinationId, CancellationToken ct) {
        var cfg = await _uow.Repository<ProxyConfigRoot>().FirstOrDefaultAsync(new ActiveProxyConfigSpec(), ct);
        if (cfg is null) return NotFound("No active proxy config found.");

        var cluster = cfg.Clusters.FirstOrDefault(c => c.ClusterId == clusterId);
        if (cluster is null) return NotFound($"Cluster '{clusterId}' not found.");

        var dest = cluster.Destinations.FirstOrDefault(d => d.DestinationId == destinationId);
        if (dest is null) return NotFound($"Destination '{destinationId}' not found in cluster '{clusterId}'.");

        dest.Disable();
        await _uow.SaveChangesAsync(ct);

        return Ok(new { Message = "Destination disabled.", clusterId, destinationId });
    }
}