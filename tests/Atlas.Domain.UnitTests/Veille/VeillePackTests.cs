using Atlas.Domain.Veille;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Veille;

public sealed class VeillePackTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_valid_input_starts_active_at_version_1()
    {
        var result = VeillePack.Create("Cabinet-PI", "Cabinet PI", "Veille PI", Now);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().Be("cabinet-pi"); // normalisé en minuscules
        result.Value!.Version.Should().Be(1);
        result.Value!.IsActive.Should().BeTrue();
        result.Value!.SourceIds.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_empty_code_fails(string code)
    {
        var result = VeillePack.Create(code, "Nom", "Desc", Now);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.invalid_veille_pack");
    }

    [Fact]
    public void Create_with_empty_name_fails()
    {
        var result = VeillePack.Create("code", "  ", "Desc", Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void SetSources_replaces_and_deduplicates()
    {
        VeillePack pack = VeillePack.Create("code", "Nom", "Desc", Now).Value!;
        var id1 = FeedSourceId.New();
        var id2 = FeedSourceId.New();

        pack.SetSources([id1, id2, id1]);

        pack.SourceIds.Should().BeEquivalentTo([id1, id2]);

        pack.SetSources([id2]);
        pack.SourceIds.Should().ContainSingle().Which.Should().Be(id2);
    }

    [Fact]
    public void BumpVersion_increments()
    {
        VeillePack pack = VeillePack.Create("code", "Nom", "Desc", Now).Value!;

        pack.BumpVersion();
        pack.BumpVersion();

        pack.Version.Should().Be(3);
    }
}
