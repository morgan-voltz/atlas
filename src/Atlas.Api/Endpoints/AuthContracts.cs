namespace Atlas.Api.Endpoints;

public sealed record RegisterRequest(string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AccessTokenResponse(string AccessToken, DateTimeOffset ExpiresAt);

public sealed record EnableTwoFactorRequest(string Code);

public sealed record VerifyTwoFactorRequest(string ChallengeToken, string Code);

public sealed record DisableTwoFactorRequest(string Code);

public sealed record ResendVerificationRequest(string Email);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(Guid UserId, string Token, string NewPassword);
