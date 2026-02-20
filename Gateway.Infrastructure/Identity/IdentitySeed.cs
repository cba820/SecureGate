using Microsoft.AspNetCore.Identity;

namespace Gateway.Infrastructure.Identity;

public static class IdentitySeed {
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager) {
        var roles = new[] { "gateway_user", "admin" };

        foreach (var role in roles) {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        const string adminUser = "admin";
        const string adminPass = "4dmInEc3rt.2026@";

        var existing = await userManager.FindByNameAsync(adminUser);
        if (existing is null) {
            var user = new ApplicationUser {
                Id = Guid.NewGuid(),
                UserName = adminUser,
                Email = "admin@local",
                EmailConfirmed = true
            };

            var created = await userManager.CreateAsync(user, adminPass);
            if (created.Succeeded) {
                await userManager.AddToRoleAsync(user, "admin");
                await userManager.AddToRoleAsync(user, "gateway_user");
            }
        }
    }
}
