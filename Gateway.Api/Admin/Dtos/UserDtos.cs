namespace Gateway.Api.Admin.Dtos {
    public record CreateUserRequest(string Email, string Password);
    public record UpdateUserRequest(string? Email, bool? LockoutEnabled);
    public record UserResponse(string Id, string Email, bool EmailConfirmed, bool LockoutEnabled, DateTimeOffset? LockoutEnd);
}
