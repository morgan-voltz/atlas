using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.AddPatentFavorite;

public sealed record AddPatentFavoriteCommand(Guid UserId, string PublicationNumber, string? Title)
    : IRequest<Result>;
