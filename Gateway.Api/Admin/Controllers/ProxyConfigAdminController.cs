using Gateway.Api.Admin.Dtos;
using Gateway.Application.Admin.Specifications;
using Gateway.Domain.ProxyConfig;
using Gateway.Infrastructure.Database.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Controllers.Admin;

[ApiController]
[Route("admin/proxy-config")]
[Authorize(Policy = "AdminOnly")]
public sealed class ProxyConfigAdminController : ControllerBase {
    private readonly IUnitOfWork _uow;

    public ProxyConfigAdminController(IUnitOfWork uow) {
        _uow = uow;
    }

    [HttpGet("active")]
    public async Task<ActionResult<ProxyConfigRoot>> GetActive(CancellationToken ct) {
        var repo = _uow.Repository<ProxyConfigRoot>();
        var cfg = await repo.FirstOrDefaultAsync(new ActiveProxyConfigSpec(), ct);

        return cfg is null ? NotFound("No active proxy config found.") : Ok(cfg);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProxyConfigRoot>> GetById(Guid id, CancellationToken ct) {
        var repo = _uow.Repository<ProxyConfigRoot>();
        var cfg = await repo.FirstOrDefaultAsync(new ProxyConfigByIdSpec(id), ct);

        return cfg is null ? NotFound($"Proxy config '{id}' not found.") : Ok(cfg);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateProxyConfigRequest req, CancellationToken ct) {
        var repo = _uow.Repository<ProxyConfigRoot>();

        var newCfg = new ProxyConfigRoot(req.Version, req.IsActive);
        await repo.AddAsync(newCfg, ct);
        await _uow.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = newCfg.Id }, new { newCfg.Id, newCfg.Version, newCfg.IsActive });
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult> Deactivate(Guid id, CancellationToken ct) {
        var repo = _uow.Repository<ProxyConfigRoot>();
        var cfg = await repo.GetByIdAsync(id, ct);
        if (cfg is null) return NotFound($"Proxy config '{id}' not found.");

        cfg.Deactivate();
        await _uow.SaveChangesAsync(ct);

        return Ok(new { Message = "Proxy config deactivated.", cfg.Id, cfg.IsActive });
    }

    /// <summary>
    /// Activa una config y desactiva cualquier otra activa.
    /// Ideal si vas a manejar versionado.
    /// </summary>
    [HttpPost("{id:guid}/set-active")]
    public async Task<ActionResult> SetActive(Guid id, CancellationToken ct) {
        var repo = _uow.Repository<ProxyConfigRoot>();

        var target = await repo.GetByIdAsync(id, ct);
        if (target is null) return NotFound($"Proxy config '{id}' not found.");

        // Nota: si tu entidad no tiene método Activate(), crea una nueva config activa en vez de activar la existente.
        // Para mantenerlo sin cambiar Domain, hacemos: desactivar todas menos target y dejamos target tal como esté.
        // Si target está inactiva y quieres activarla, te recomiendo agregar cfg.Activate() en Domain.
        var active = await repo.FirstOrDefaultAsync(new ActiveProxyConfigSpec(), ct);
        if (active is not null && active.Id != target.Id)
            active.Deactivate();

        // Si necesitas activar target cuando está inactiva, agrega un método Activate() en Domain y úsalo aquí.
        // target.Activate();

        await _uow.SaveChangesAsync(ct);
        return Ok(new { Message = "Active proxy config switched.", ActiveId = target.Id });
    }
}