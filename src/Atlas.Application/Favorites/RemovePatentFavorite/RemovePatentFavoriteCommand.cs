using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.RemovePatentFavorite;

public sealed record RemovePatentFavoriteCommand(Guid UserId, string PublicationNumber) : IRequest<Result>;
