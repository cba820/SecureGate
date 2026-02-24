using Gateway.AdminUI.Dtos;
using Gateway.AdminUI.Models;
using Gateway.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.AdminUI.Controllers {
    public class AuthController : Controller {
        private readonly GatewayApiClient _api;

        public AuthController(GatewayApiClient api) => _api = api;

        [HttpGet]
        public IActionResult Login() => View(new LoginVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm) {
            if (!ModelState.IsValid) return View(vm);

            var result = await _api.LoginAsync(new LoginRequest(vm.Email, vm.Password));
            if (!result.Ok || result.Data is null) {
                ModelState.AddModelError("", result.Error ?? "Login fallido.");
                return View(vm);
            }

            HttpContext.Session.SetString("ADMIN_JWT", result.Data.AccessToken);
            HttpContext.Session.SetString("ADMIN_JWT_EXP", result.Data.ExpiresAt.ToString("O"));

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout() {
            HttpContext.Session.Remove("ADMIN_JWT");
            HttpContext.Session.Remove("ADMIN_JWT_EXP");
            return RedirectToAction("Login");
        }
    }
}
