using Atlas.Application.Notifications;
using Atlas.Application.Notifications.GetMyDevices;
using Atlas.Application.Notifications.RegisterDevice;
using Atlas.Application.Notifications.UnregisterDevice;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

/// <summary>
/// Endpoints d'enregistrement des devices pour le push (F-020).
/// Le token plateforme (FCM/APNs/WNS) est envoyé par le client à chaque démarrage / refresh
/// de token. Le backend déduplique par token.
/// </summary>
internal static class DevicesEndpoints
{
    public static IEndpointRouteBuilder MapDevicesEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/devices")
            .WithTags("Devices")
            .RequireAuthorization();

        group.MapPost("", RegisterAsync);
        group.MapDelete("{id:guid}", UnregisterAsync);
        group.MapGet("", GetMineAsync);

        return routes;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterDeviceRequest request,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result<Guid> result = await sender.Send(
            new RegisterDeviceCommand(user.Id, request.Platform, request.Token, request.Label),
            ct);

        return result.IsSuccess
            ? Results.Ok(new { id = result.Value })
            : result.Error!.ToProblem();
    }

    private static async Task<IResult> UnregisterAsync(
        Guid id,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(new UnregisterDeviceCommand(user.Id, id), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetMineAsync(
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result<IReadOnlyList<DeviceRegistrationDto>> result =
            await sender.Send(new GetMyDevicesQuery(user.Id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }
}

internal sealed record RegisterDeviceRequest(string Platform, string Token, string? Label);
