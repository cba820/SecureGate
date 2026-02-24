using Gateway.Api.Admin.Dtos;
using Gateway.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Admin.Controllers;

[ApiController]
[Route("_admin/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public UsersController(UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole<Guid>> roleManager) {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserResponse>> GetAll() {
        var users = _userManager.Users
            .OrderBy(u => u.Email)
            .Select(u => new UserResponse(
                u.Id.ToString(),
                u.Email ?? "",
                u.EmailConfirmed,
                u.LockoutEnabled,
                u.LockoutEnd))
            .ToList();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetById(string id) {
        var u = await _userManager.FindByIdAsync(id);
        if (u is null) return NotFound();

        return Ok(new UserResponse(u.Id.ToString(), u.Email ?? "", u.EmailConfirmed, u.LockoutEnabled, u.LockoutEnd));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest req) {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email and password are required.");

        var email = req.Email.Trim();

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
            return Conflict("Email already exists.");

        // Reemplaza la creación de IdentityUser por ApplicationUser en el método Create
        var user = new ApplicationUser {
            UserName = email,
            Email = email,
            EmailConfirmed = true // MVP: si quieres confirmación real, lo cambiamos
        };

        var result = await _userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok(new UserResponse(user.Id.ToString(), user.Email!, user.EmailConfirmed, user.LockoutEnabled, user.LockoutEnd));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateUserRequest req) {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(req.Email)) {
            var email = req.Email.Trim();
            user.Email = email;
            user.UserName = email;
        }

        if (req.LockoutEnabled.HasValue)
            user.LockoutEnabled = req.LockoutEnabled.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id) {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return NoContent();
    }

    // Roles del usuario
    [HttpGet("{id}/roles")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(string id) {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(roles.OrderBy(r => r));
    }

    [HttpPost("{id}/roles/{roleName}")]
    public async Task<IActionResult> AddRole(string id, string roleName) {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (!await _roleManager.RoleExistsAsync(roleName))
            return NotFound("Role not found.");

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return NoContent();
    }

    [HttpDelete("{id}/roles/{roleName}")]
    public async Task<IActionResult> RemoveRole(string id, string roleName) {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return NoContent();
    }
}