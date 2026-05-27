using Atlas.Application.Common;
using Atlas.Application.Users;
using Atlas.Application.Users.Login;
using Atlas.Application.Users.Logout;
using Atlas.Application.Users.Refresh;
using Atlas.Application.Users.Register;
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

        group.MapPost("/register", RegisterAsync);
        group.MapGet("/verify-email", VerifyEmailAsync);
        group.MapPost("/login", LoginAsync);
        group.MapPost("/refresh", RefreshAsync);
        group.MapPost("/logout", LogoutAsync);

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
        Result<AuthTokensDto> result = await sender.Send(new LoginCommand(request.Email, request.Password), ct);
        if (result.IsFailure)
        {
            return result.Error!.ToProblem();
        }

        AuthTokensDto tokens = result.Value!;
        SetRefreshCookie(httpContext, tokens.RefreshToken, settings.RefreshTokenLifetime);

        return Results.Ok(new AccessTokenResponse(tokens.AccessToken, tokens.AccessTokenExpiresAt));
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
