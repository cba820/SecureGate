namespace Gateway.Api.Admin.Dtos {
    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt);
}
