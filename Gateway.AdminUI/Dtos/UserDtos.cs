namespace Gateway.AdminUI.Dtos {
    public record CreateUserRequest(string Email, string Password);
    public record UserResponse(string Id, string Email, bool EmailConfirmed, bool LockoutEnabled, DateTimeOffset? LockoutEnd);
}
