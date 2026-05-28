using Atlas.Application.Common;
using Atlas.Shared.Result;

namespace Atlas.Api.Endpoints;

/// <summary>
/// Traduit un <see cref="Error"/> métier en réponse HTTP (ProblemDetails) avec le statut approprié.
/// </summary>
internal static class ErrorHttpMapping
{
    public static IResult ToProblem(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error is ValidationError validation)
        {
            var errors = validation.Failures
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.Message).ToArray());

            return Results.ValidationProblem(errors, detail: error.Message, type: error.Code);
        }

        int status = StatusCodeFor(error.Code);
        return Results.Problem(detail: error.Message, statusCode: status, title: error.Code);
    }

    private static int StatusCodeFor(string code) => code switch
    {
        "users.email_already_in_use" => StatusCodes.Status409Conflict,
        "users.invalid_credentials" => StatusCodes.Status401Unauthorized,
        "users.email_not_verified" => StatusCodes.Status403Forbidden,
        "users.account_locked" => StatusCodes.Status423Locked,
        "users.invalid_refresh_token" => StatusCodes.Status401Unauthorized,
        "users.not_found" => StatusCodes.Status404NotFound,
        "users.two_factor_already_enabled" => StatusCodes.Status409Conflict,
        "users.two_factor_not_enabled" => StatusCodes.Status409Conflict,
        "users.two_factor_setup_not_started" => StatusCodes.Status409Conflict,
        "users.invalid_two_factor_code" => StatusCodes.Status401Unauthorized,
        "users.invalid_two_factor_challenge" => StatusCodes.Status401Unauthorized,
        "inpi.invalid_credentials" => StatusCodes.Status400BadRequest,
        "inpi.api_access_not_allowed" => StatusCodes.Status403Forbidden,
        "inpi.unavailable" => StatusCodes.Status502BadGateway,
        "inpi.not_connected" => StatusCodes.Status409Conflict,
        "companies.invalid_siren" => StatusCodes.Status400BadRequest,
        "companies.not_found" => StatusCodes.Status404NotFound,
        "trademarks.not_found" => StatusCodes.Status404NotFound,
        "trademarks.image_not_found" => StatusCodes.Status404NotFound,
        "veille.subscription_not_found" => StatusCodes.Status404NotFound,
        "veille.veille_pack_not_found" => StatusCodes.Status404NotFound,
        "veille.feed_item_not_found" => StatusCodes.Status404NotFound,
        "veille.already_subscribed" => StatusCodes.Status409Conflict,
        "veille.veille_pack_not_enrolled" => StatusCodes.Status409Conflict,
        "veille.subscription_limit_reached" => StatusCodes.Status409Conflict,
        "veille.source_blocked" => StatusCodes.Status403Forbidden,
        "veille.fetch_failed" => StatusCodes.Status502BadGateway,
        "favorites.company_already_favorite" => StatusCodes.Status409Conflict,
        "favorites.company_not_favorite" => StatusCodes.Status404NotFound,
        "favorites.invalid_siren" => StatusCodes.Status400BadRequest,
        // veille.feed_unreachable / veille.invalid_feed_source / veille.invalid_veille_pack
        // restent en 400 (URL/saisie utilisateur invalide) via le défaut.
        _ => StatusCodes.Status400BadRequest,
    };
}
