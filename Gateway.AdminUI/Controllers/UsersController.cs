using Gateway.AdminUI.Dtos;
using Gateway.AdminUI.Filters;
using Gateway.AdminUI.Models;
using Gateway.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.AdminUI.Controllers {
    [RequireAdminAuth]
    public class UsersController : Controller {
        private readonly GatewayApiClient _api;

        public UsersController(GatewayApiClient api) => _api = api;

        [HttpGet]
        public async Task<IActionResult> Index() {
            var usersRes = await _api.GetUsersAsync();
            if (!usersRes.Ok || usersRes.Data is null) {
                TempData["Err"] = $"No se pudieron cargar usuarios: {usersRes.Error}";
                return View(new UsersIndexVm()); // vacío, pero la vista carga
            }

            var rolesRes = await _api.GetRolesAsync();
            if (!rolesRes.Ok || rolesRes.Data is null) {
                TempData["Err"] = $"No se pudieron cargar roles: {rolesRes.Error}";
                return View(new UsersIndexVm {
                    Users = usersRes.Data
                });
            }

            var users = usersRes.Data;
            var roles = rolesRes.Data;

            // MVP: cargar roles por usuario (N llamadas). Luego lo optimizamos con endpoint agregado.
            var userRoles = new Dictionary<string, List<string>>();
            foreach (var u in users) {
                var urRes = await _api.GetUserRolesAsync(u.Id);
                userRoles[u.Id] = (urRes.Ok && urRes.Data is not null)
                    ? urRes.Data
                    : new List<string>();
            }

            return View(new UsersIndexVm {
                Users = users,
                AllRoles = roles.Select(r => r.Name).OrderBy(x => x).ToList(),
                UserRoles = userRoles
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsersCreateVm vm) {
            if (!ModelState.IsValid) {
                TempData["Err"] = "Datos inválidos.";
                return RedirectToAction(nameof(Index));
            }

            var res = await _api.CreateUserAsync(new CreateUserRequest(vm.Email.Trim(), vm.Password));

            if (!res.Ok)
                TempData["Err"] = $"No se pudo crear usuario: {res.Error}";
            else
                TempData["Ok"] = "Usuario creado.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id) {
            var res = await _api.DeleteUserAsync(id);

            if (!res.Ok)
                TempData["Err"] = $"No se pudo eliminar: {res.Error}";
            else
                TempData["Ok"] = "Usuario eliminado.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string userId, string roleName) {
            var res = await _api.AddUserRoleAsync(userId, roleName);

            if (!res.Ok)
                TempData["Err"] = $"No se pudo asignar rol: {res.Error}";
            else
                TempData["Ok"] = "Rol asignado.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string roleName) {
            var res = await _api.RemoveUserRoleAsync(userId, roleName);

            if (!res.Ok)
                TempData["Err"] = $"No se pudo quitar rol: {res.Error}";
            else
                TempData["Ok"] = "Rol removido.";

            return RedirectToAction(nameof(Index));
        }
    }
}