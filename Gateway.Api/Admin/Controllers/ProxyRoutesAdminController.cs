using Gateway.Api.Admin.Dtos;
using Gateway.Application.Admin.Specifications;
using Gateway.Domain.ProxyConfig;
using Gateway.Infrastructure.Database.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Controllers.Admin;

[ApiController]
[Route("admin/proxy-config/routes")]
[Authorize(Policy = "AdminOnly")]
public sealed class ProxyRoutesAdminController : ControllerBase {
    private readonly IUnitOfWork _uow;

    public ProxyRoutesAdminController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    public async Task<ActionResult> ListActiveConfigRoutes(CancellationToken ct) {
        var cfg = await _uow.Repository<ProxyConfigRoot>().FirstOrDefaultAsync(new ActiveProxyConfigSpec(), ct);
        if (cfg is null) return NotFound("No active proxy config found.");

        return Ok(cfg.Routes.Where(r => r.IsActive));
    }

    [HttpPost]
    public async Task<ActionResult> AddRoute([FromBody] AddRouteRequest req, CancellationToken ct) {
        var cfgRepo = _uow.Repository<ProxyConfigRoot>();
        var cfg = await cfgRepo.FirstOrDefaultAsync(new ActiveProxyConfigSpec(), ct);
        if (cfg is null) return NotFound("No active proxy config found.");

        // Validación mínima: cluster debe existir en la config
        var clusterExists = cfg.Clusters.Any(c => c.ClusterId == req.ClusterId && c.IsActive);
        if (!clusterExists)
            return BadRequest($"Cluster '{req.ClusterId}' does not exist (or is inactive) in the active config.");

        var route = new ProxyRoute(req.RouteId, req.ClusterId, req.PathMatch, req.AuthorizationPolicy);

        if (req.Transforms is not null) {
            foreach (var t in req.Transforms)
                route.AddTransform(new RouteTransform(t.Key, t.Value));
        }

        cfg.AddRoute(route);
        await _uow.SaveChangesAsync(ct);

        return Ok(new { Message = "Route added.", route.RouteId, route.ClusterId });
    }

    [HttpDelete("{routeId}")]
    public async Task<ActionResult> DisableRoute(string routeId, CancellationToken ct) {
        var cfg = await _uow.Repository<ProxyConfigRoot>().FirstOrDefaultAsync(new ActiveProxyConfigSpec(), ct);
        if (cfg is null) return NotFound("No active proxy config found.");

        var route = cfg.Routes.FirstOrDefault(r => r.RouteId == routeId);
        if (route is null) return NotFound($"Route '{routeId}' not found.");

        route.Disable();
        await _uow.SaveChangesAsync(ct);

        return Ok(new { Message = "Route disabled.", routeId });
    }
}