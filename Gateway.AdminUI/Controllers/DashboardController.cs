using Gateway.AdminUI.Filters;
using Gateway.AdminUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.AdminUI.Controllers {
    [RequireAdminAuth]
    public class DashboardController : Controller {
        private readonly GatewayApiClient _api;

        public DashboardController(GatewayApiClient api) => _api = api;

        [HttpGet]
        public async Task<IActionResult> Index() {

            return View();
        }
    }
}
