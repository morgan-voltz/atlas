using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using MediatR;

namespace Atlas.Application.Favorites.FavoriteRefresh;

/// <summary>
/// Publiée par <see cref="RefreshFavoritesHandler"/> quand un favori d'un utilisateur a évolué (F-019).
/// Plusieurs handlers s'abonnent : envoi d'email, dispatch push, …
/// </summary>
public sealed record CompanyFavoriteChangedNotification(
    UserId UserId,
    EmailAddress UserEmail,
    string SirenValue,
    string? Denomination,
    IReadOnlyList<CompanyFavoriteChange> Changes) : INotification;
