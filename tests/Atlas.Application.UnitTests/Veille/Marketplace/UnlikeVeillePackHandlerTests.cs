using Atlas.Application.Veille.Marketplace.UnlikeVeillePack;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille.Marketplace;

public sealed class UnlikeVeillePackHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IVeillePackRepository _packs = Substitute.For<IVeillePackRepository>();
    private readonly IVeillePackLikeRepository _likes = Substitute.For<IVeillePackLikeRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private UnlikeVeillePackHandler CreateHandler() => new(_packs, _likes, _unitOfWork);

    private static VeillePack PublishedPack(UserId author)
    {
        VeillePack pack = VeillePack.CreateUserPack(author, "p", "P", string.Empty, Now).Value!;
        pack.Publish();
        return pack;
    }

    [Fact]
    public async Task Handle_removes_like_and_decrements_counter_atomically()
    {
        VeillePack pack = PublishedPack(new UserId(Guid.NewGuid()));
        var userId = Guid.NewGuid();
        _packs.GetByCodeAsync("p", Arg.Any<CancellationToken>()).Returns(pack);
        _likes.GetAsync(new UserId(userId), pack.Id, Arg.Any<CancellationToken>())
            .Returns(VeillePackLike.Create(pack.Id, new UserId(userId), Now));

        Result result = await CreateHandler().Handle(
            new UnlikeVeillePackCommand(userId, "p"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _likes.Received(1).RemoveAsync(Arg.Any<VeillePackLike>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _packs.Received(1).DecrementLikesAsync(pack.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_is_idempotent_when_not_liked()
    {
        VeillePack pack = PublishedPack(new UserId(Guid.NewGuid()));
        _packs.GetByCodeAsync("p", Arg.Any<CancellationToken>()).Returns(pack);
        _likes.GetAsync(Arg.Any<UserId>(), Arg.Any<VeillePackId>(), Arg.Any<CancellationToken>())
            .Returns((VeillePackLike?)null);

        Result result = await CreateHandler().Handle(
            new UnlikeVeillePackCommand(Guid.NewGuid(), "p"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _packs.DidNotReceive().DecrementLikesAsync(Arg.Any<VeillePackId>(), Arg.Any<CancellationToken>());
        await _likes.DidNotReceive().RemoveAsync(Arg.Any<VeillePackLike>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_not_found_when_pack_missing()
    {
        _packs.GetByCodeAsync("missing", Arg.Any<CancellationToken>()).Returns((VeillePack?)null);

        Result result = await CreateHandler().Handle(
            new UnlikeVeillePackCommand(Guid.NewGuid(), "missing"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.veille_pack_not_found");
    }
}
