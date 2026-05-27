namespace Atlas.Api.Endpoints;

public sealed record RegisterRequest(string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AccessTokenResponse(string AccessToken, DateTimeOffset ExpiresAt);
