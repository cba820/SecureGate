using Gateway.AdminUI.Dtos;
using Gateway.AdminUI.Filters;
using Gateway.AdminUI.Models;
using Gateway.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.AdminUI.Controllers {
    [RequireAdminAuth]
    public class RolesController : Controller {
        private readonly GatewayApiClient _api;

        public RolesController(GatewayApiClient api) => _api = api;

        [HttpGet]
        public async Task<IActionResult> Index() {
            var rolesRes = await _api.GetRolesAsync();

            if (!rolesRes.Ok || rolesRes.Data is null) {
                TempData["Err"] = $"No se pudieron cargar roles: {rolesRes.Error}";
                return View(new RolesIndexVm { Roles = new List<RoleResponse>() });
            }

            return View(new RolesIndexVm { Roles = rolesRes.Data });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RolesIndexVm vm) {
            if (string.IsNullOrWhiteSpace(vm.NewRoleName)) {
                TempData["Err"] = "Nombre de rol requerido.";
                return RedirectToAction(nameof(Index));
            }

            var res = await _api.CreateRoleAsync(vm.NewRoleName.Trim());

            if (!res.Ok)
                TempData["Err"] = $"No se pudo crear: {res.Error}";
            else
                TempData["Ok"] = "Rol creado.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string roleName) {
            if (string.IsNullOrWhiteSpace(roleName)) {
                TempData["Err"] = "Rol inválido.";
                return RedirectToAction(nameof(Index));
            }

            var res = await _api.DeleteRoleAsync(roleName);

            if (!res.Ok)
                TempData["Err"] = $"No se pudo eliminar: {res.Error}";
            else
                TempData["Ok"] = "Rol eliminado.";

            return RedirectToAction(nameof(Index));
        }
    }
}