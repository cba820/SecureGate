using Gateway.Api.Auth.Dtos;
using Gateway.Api.Auth.Services;
using Gateway.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Gateway.Api.Endpoints;

public static class AuthEndpoints {
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app) {
        var group = app.MapGroup("/auth");

        group.MapPost("/token", async (
            TokenRequest req,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtTokenService tokens,
            CancellationToken ct) => {
                var user = await userManager.FindByNameAsync(req.Username);
                if (user is null)
                    return Results.Unauthorized();

                // Valida password + aplica lockout si quieres
                var result = await signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                    return Results.Unauthorized();

                var roles = await userManager.GetRolesAsync(user);

                // Subject: usa UserName o Id según prefieras
                var response = tokens.CreateAccessToken(subject: user.UserName ?? user.Id.ToString(), roles: roles);

                return Results.Ok(response);
            }).AllowAnonymous();

        group.MapGet("/me", [Authorize] (System.Security.Claims.ClaimsPrincipal user) => {
            var name = user.Identity?.Name ?? user.FindFirst("sub")?.Value;
            var roles = user.Claims
                .Where(c => c.Type.EndsWith("/role") || c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct()
                .ToArray();

            return Results.Ok(new { subject = name, roles });
        });

        return app;
    }
}