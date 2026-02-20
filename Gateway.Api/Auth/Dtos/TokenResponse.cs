namespace Gateway.Api.Auth.Dtos;

public sealed record TokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresInSeconds
);
