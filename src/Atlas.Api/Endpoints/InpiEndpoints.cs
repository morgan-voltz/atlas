using System.Security.Claims;
using Atlas.Application.Inpi;
using Atlas.Application.Inpi.ConnectInpiAccount;
using Atlas.Application.Inpi.DisconnectInpiAccount;
using Atlas.Application.Inpi.GetInpiConnectionStatus;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class InpiEndpoints
{
    public static IEndpointRouteBuilder MapInpiEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/inpi").WithTags("INPI").RequireAuthorization();

        group.MapPost("/connection", ConnectAsync);
        group.MapGet("/connection", StatusAsync);
        group.MapDelete("/connection", DisconnectAsync);

        return routes;
    }

    private static async Task<IResult> ConnectAsync(
        ConnectInpiRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result result = await sender.Send(
            new ConnectInpiAccountCommand(userId, request.Username, request.Password), ct);

        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> StatusAsync(
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<InpiConnectionStatusDto> result = await sender.Send(new GetInpiConnectionStatusQuery(userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> DisconnectAsync(
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result result = await sender.Send(new DisconnectInpiAccountCommand(userId), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }
}
