using Microsoft.AspNetCore.Identity;

namespace Gateway.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid> {
    // Extiende aquí si quieres:
    // public string? DisplayName { get; set; }
}
