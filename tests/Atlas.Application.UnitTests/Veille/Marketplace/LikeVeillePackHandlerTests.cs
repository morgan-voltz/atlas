using Atlas.Application.Veille.Marketplace.LikeVeillePack;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille.Marketplace;

public sealed class LikeVeillePackHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IVeillePackRepository _packs = Substitute.For<IVeillePackRepository>();
    private readonly IVeillePackLikeRepository _likes = Substitute.For<IVeillePackLikeRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public LikeVeillePackHandlerTests() => _clock.UtcNow.Returns(Now);

    private LikeVeillePackHandler CreateHandler() => new(_packs, _likes, _clock, _unitOfWork);

    private static VeillePack PublishedPack(UserId author)
    {
        VeillePack pack = VeillePack.CreateUserPack(author, "p", "P", string.Empty, Now).Value!;
        pack.Publish();
        return pack;
    }

    [Fact]
    public async Task Handle_increments_counter_and_persists_like()
    {
        VeillePack pack = PublishedPack(new UserId(Guid.NewGuid()));
        _packs.GetByCodeAsync("p", Arg.Any<CancellationToken>()).Returns(pack);
        _likes.ExistsAsync(Arg.Any<UserId>(), Arg.Any<VeillePackId>(), Arg.Any<CancellationToken>()).Returns(false);

        Result result = await CreateHandler().Handle(
            new LikeVeillePackCommand(Guid.NewGuid(), "p"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        pack.LikesCount.Should().Be(1);
        await _likes.Received(1).AddAsync(Arg.Any<VeillePackLike>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_is_idempotent_when_already_liked()
    {
        VeillePack pack = PublishedPack(new UserId(Guid.NewGuid()));
        _packs.GetByCodeAsync("p", Arg.Any<CancellationToken>()).Returns(pack);
        _likes.ExistsAsync(Arg.Any<UserId>(), Arg.Any<VeillePackId>(), Arg.Any<CancellationToken>()).Returns(true);

        Result result = await CreateHandler().Handle(
            new LikeVeillePackCommand(Guid.NewGuid(), "p"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        pack.LikesCount.Should().Be(0, "déjà liké → pas de double-comptage");
        await _likes.DidNotReceive().AddAsync(Arg.Any<VeillePackLike>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_not_public_when_pack_is_private()
    {
        VeillePack pack = VeillePack.CreateUserPack(
            new UserId(Guid.NewGuid()), "p", "P", string.Empty, Now).Value!;
        // Pas de publish() → reste Private
        _packs.GetByCodeAsync("p", Arg.Any<CancellationToken>()).Returns(pack);

        Result result = await CreateHandler().Handle(
            new LikeVeillePackCommand(Guid.NewGuid(), "p"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.veille_pack_not_public");
    }

    [Fact]
    public async Task Handle_returns_not_found_when_pack_missing()
    {
        _packs.GetByCodeAsync("missing", Arg.Any<CancellationToken>()).Returns((VeillePack?)null);

        Result result = await CreateHandler().Handle(
            new LikeVeillePackCommand(Guid.NewGuid(), "missing"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.veille_pack_not_found");
    }
}
