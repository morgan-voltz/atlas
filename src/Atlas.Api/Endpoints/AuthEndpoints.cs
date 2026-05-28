using System.Security.Claims;
using Atlas.Api.Security;
using Atlas.Application.Common;
using Atlas.Application.Users;
using Atlas.Application.Users.Login;
using Atlas.Application.Users.Logout;
using Atlas.Application.Users.Refresh;
using Atlas.Application.Users.Register;
using Atlas.Application.Users.TwoFactor;
using Atlas.Application.Users.VerifyEmail;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class AuthEndpoints
{
    private const string RefreshCookieName = "atlas_refresh";
    private const string CookiePath = "/auth";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/auth").WithTags("Auth");

        // Rate limiting strict (Lot 2b) sur les endpoints exposés à la force brute :
        // register (énumération de comptes), login + refresh + 2fa/verify (devinette de
        // mot de passe / code TOTP / token de refresh).
        group.MapPost("/register", RegisterAsync).RequireRateLimiting(RateLimitOptions.AuthStrictPolicy);
        group.MapGet("/verify-email", VerifyEmailAsync);
        group.MapPost("/login", LoginAsync).RequireRateLimiting(RateLimitOptions.AuthStrictPolicy);
        group.MapPost("/refresh", RefreshAsync).RequireRateLimiting(RateLimitOptions.AuthStrictPolicy);
        group.MapPost("/logout", LogoutAsync);

        group.MapPost("/2fa/setup", SetupTwoFactorAsync).RequireAuthorization();
        group.MapPost("/2fa/enable", EnableTwoFactorAsync).RequireAuthorization();
        group.MapPost("/2fa/disable", DisableTwoFactorAsync).RequireAuthorization();
        group.MapPost("/2fa/verify", VerifyTwoFactorAsync).RequireRateLimiting(RateLimitOptions.AuthStrictPolicy);

        return routes;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(new RegisterUserCommand(request.Email, request.Password), ct);

        return result.IsSuccess
            ? Results.Ok(new { message = "Compte créé. Vérifiez votre email pour activer votre compte." })
            : result.Error!.ToProblem();
    }

    private static async Task<IResult> VerifyEmailAsync(
        Guid userId,
        string token,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(new VerifyEmailCommand(userId, token), ct);

        return result.IsSuccess
            ? Results.Ok(new { message = "Email vérifié. Votre compte est actif." })
            : result.Error!.ToProblem();
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        ISender sender,
        HttpContext httpContext,
        AuthSettings settings,
        CancellationToken ct)
    {
        Result<LoginResultDto> result = await sender.Send(new LoginCommand(request.Email, request.Password), ct);
        if (result.IsFailure)
        {
            return result.Error!.ToProblem();
        }

        LoginResultDto login = result.Value!;
        if (login.TwoFactorRequired)
        {
            return Results.Ok(new { twoFactorRequired = true, challengeToken = login.TwoFactorChallengeToken });
        }

        SetRefreshCookie(httpContext, login.Tokens!.RefreshToken, settings.RefreshTokenLifetime);
        return Results.Ok(new AccessTokenResponse(login.Tokens.AccessToken, login.Tokens.AccessTokenExpiresAt));
    }

    private static async Task<IResult> RefreshAsync(
        ISender sender,
        HttpContext httpContext,
        AuthSettings settings,
        CancellationToken ct)
    {
        if (!httpContext.Request.Cookies.TryGetValue(RefreshCookieName, out string? refreshToken)
            || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Results.Unauthorized();
        }

        Result<AuthTokensDto> result = await sender.Send(new RefreshTokenCommand(refreshToken), ct);
        if (result.IsFailure)
        {
            DeleteRefreshCookie(httpContext);
            return result.Error!.ToProblem();
        }

        AuthTokensDto tokens = result.Value!;
        SetRefreshCookie(httpContext, tokens.RefreshToken, settings.RefreshTokenLifetime);

        return Results.Ok(new AccessTokenResponse(tokens.AccessToken, tokens.AccessTokenExpiresAt));
    }

    private static async Task<IResult> LogoutAsync(
        ISender sender,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (httpContext.Request.Cookies.TryGetValue(RefreshCookieName, out string? refreshToken)
            && !string.IsNullOrWhiteSpace(refreshToken))
        {
            await sender.Send(new LogoutCommand(refreshToken), ct);
        }

        DeleteRefreshCookie(httpContext);
        return Results.NoContent();
    }

    private static async Task<IResult> SetupTwoFactorAsync(
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<TwoFactorSetupDto> result = await sender.Send(new SetupTwoFactorCommand(userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> EnableTwoFactorAsync(
        EnableTwoFactorRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<TwoFactorEnabledDto> result = await sender.Send(new EnableTwoFactorCommand(userId, request.Code), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> DisableTwoFactorAsync(
        DisableTwoFactorRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result result = await sender.Send(new DisableTwoFactorCommand(userId, request.Code), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> VerifyTwoFactorAsync(
        VerifyTwoFactorRequest request,
        ISender sender,
        HttpContext httpContext,
        AuthSettings settings,
        CancellationToken ct)
    {
        Result<AuthTokensDto> result =
            await sender.Send(new VerifyTwoFactorCommand(request.ChallengeToken, request.Code), ct);
        if (result.IsFailure)
        {
            return result.Error!.ToProblem();
        }

        AuthTokensDto tokens = result.Value!;
        SetRefreshCookie(httpContext, tokens.RefreshToken, settings.RefreshTokenLifetime);
        return Results.Ok(new AccessTokenResponse(tokens.AccessToken, tokens.AccessTokenExpiresAt));
    }

    private static void SetRefreshCookie(HttpContext httpContext, string token, TimeSpan lifetime)
    {
        httpContext.Response.Cookies.Append(RefreshCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = CookiePath,
            MaxAge = lifetime,
            IsEssential = true,
        });
    }

    private static void DeleteRefreshCookie(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = CookiePath,
        });
    }
}
