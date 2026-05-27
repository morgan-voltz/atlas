using System.Security.Claims;

namespace Atlas.Api.Endpoints;

internal static class ClaimsPrincipalExtensions
{
    private const string SubClaim = "sub";

    public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId)
    {
        ArgumentNullException.ThrowIfNull(principal);

        userId = Guid.Empty;
        string? subject = principal.FindFirstValue(SubClaim);
        return subject is not null && Guid.TryParse(subject, out userId);
    }
}
