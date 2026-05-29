using Atlas.Application.Veille.Marketplace;
using Atlas.Application.Veille.Marketplace.PublishVeillePack;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille.Marketplace;

public sealed class PublishVeillePackHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IVeillePackRepository _packs = Substitute.For<IVeillePackRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private PublishVeillePackHandler CreateHandler() => new(_packs, _unitOfWork);

    [Fact]
    public async Task Handle_author_publishes_successfully()
    {
        Guid authorId = Guid.NewGuid();
        VeillePack pack = VeillePack.CreateUserPack(new UserId(authorId), "p", "P", string.Empty, Now).Value!;
        _packs.GetByCodeAsync("p", Arg.Any<CancellationToken>()).Returns(pack);

        Result<VeillePackMarketplaceDto> result = await CreateHandler().Handle(
            new PublishVeillePackCommand(authorId, "p"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        pack.Visibility.Should().Be(VeillePackVisibility.Public);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_forbidden_when_caller_not_author()
    {
        VeillePack pack = VeillePack.CreateUserPack(
            new UserId(Guid.NewGuid()), "p", "P", string.Empty, Now).Value!;
        _packs.GetByCodeAsync("p", Arg.Any<CancellationToken>()).Returns(pack);

        Result<VeillePackMarketplaceDto> result = await CreateHandler().Handle(
            new PublishVeillePackCommand(Guid.NewGuid(), "p"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.veille_pack_forbidden");
        pack.Visibility.Should().Be(VeillePackVisibility.Private, "non publié quand caller refusé");
    }

    [Fact]
    public async Task Handle_fails_on_system_pack()
    {
        VeillePack systemPack = VeillePack.Create("cabinet-pi", "Cabinet PI", string.Empty, Now).Value!;
        _packs.GetByCodeAsync("cabinet-pi", Arg.Any<CancellationToken>()).Returns(systemPack);

        Result<VeillePackMarketplaceDto> result = await CreateHandler().Handle(
            new PublishVeillePackCommand(Guid.NewGuid(), "cabinet-pi"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.veille_pack_forbidden",
            "pack système n'a pas d'auteur user → refusé en amont par la garde author check");
    }
}
