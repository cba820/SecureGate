using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gateway.AdminUI.Filters {
    public class RequireAdminAuthAttribute : ActionFilterAttribute {
        public override void OnActionExecuting(ActionExecutingContext context) {
            var token = context.HttpContext.Session.GetString("ADMIN_JWT");
            if (string.IsNullOrWhiteSpace(token)) {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
