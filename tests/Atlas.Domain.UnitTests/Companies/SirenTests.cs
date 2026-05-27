using Atlas.Domain.Companies;
using Atlas.Shared.Result;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Companies;

public class SirenTests
{
    [Theory]
    [InlineData("552032534")]   // Renault SA
    [InlineData("552 032 534")] // espaces tolérés
    public void Create_with_valid_siren_succeeds_and_normalizes(string raw)
    {
        Result<Siren> result = Siren.Create(raw);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().Be("552032534");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]        // mauvaise longueur
    [InlineData("55203253A")]    // caractère non numérique
    [InlineData("552032535")]    // clé de Luhn invalide
    public void Create_with_invalid_siren_fails(string? raw)
    {
        Result<Siren> result = Siren.Create(raw);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("companies.invalid_siren");
    }
}
