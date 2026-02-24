using Gateway.Api.Admin.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Admin.Controllers;

[ApiController]
[Route("_admin/roles")]
[Authorize(Policy = "AdminOnly")]
public class RolesController : ControllerBase {
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public RolesController(RoleManager<IdentityRole<Guid>> roleManager) {
        _roleManager = roleManager;
    }

    [HttpGet]
    public ActionResult<IEnumerable<RoleResponse>> GetAll() {
        var roles = _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => new RoleResponse(r.Id.ToString(), r.Name!))
            .ToList();

        return Ok(roles);
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create(CreateRoleRequest req) {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest("Role name is required.");

        var name = req.Name.Trim();

        if (await _roleManager.RoleExistsAsync(name))
            return Conflict("Role already exists.");

        var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(name));
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        var created = await _roleManager.FindByNameAsync(name);
        return Ok(new RoleResponse(created!.Id.ToString(), created.Name!));
    }

    [HttpDelete("{roleName}")]
    public async Task<IActionResult> Delete(string roleName) {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role is null) return NotFound();

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return NoContent();
    }
}