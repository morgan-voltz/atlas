using Atlas.Domain.Veille;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Veille;

public sealed class SimHashTests
{
    [Fact]
    public void Compute_is_deterministic_for_the_same_text()
    {
        const string text = "Rachat de la société Atlas par un grand groupe industriel";

        SimHash.Compute(text).Should().Be(SimHash.Compute(text));
    }

    [Fact]
    public void Identical_text_has_zero_distance()
    {
        long a = SimHash.Compute("La société Atlas lève 10 millions d'euros");
        long b = SimHash.Compute("La société Atlas lève 10 millions d'euros");

        SimHash.HammingDistance(a, b).Should().Be(0);
    }

    [Fact]
    public void Normalization_ignores_case_accents_and_punctuation()
    {
        long a = SimHash.Compute("La Société ATLAS lève 10 Millions d'euros !");
        long b = SimHash.Compute("la societe atlas leve 10 millions d euros");

        SimHash.HammingDistance(a, b).Should().Be(0);
    }

    [Fact]
    public void Near_duplicate_is_closer_than_an_unrelated_subject()
    {
        long reference = SimHash.Compute("Le groupe Atlas rachète la startup Beta pour 50 millions d'euros");
        long nearDuplicate = SimHash.Compute("Atlas rachète la startup Beta pour 50 millions d'euros, annonce le groupe");
        long unrelated = SimHash.Compute("La météo sera pluvieuse sur la côte atlantique ce week-end");

        int nearDistance = SimHash.HammingDistance(reference, nearDuplicate);
        int unrelatedDistance = SimHash.HammingDistance(reference, unrelated);

        nearDistance.Should().BeLessThan(unrelatedDistance);
    }

    [Fact]
    public void Empty_or_null_text_yields_zero_fingerprint()
    {
        SimHash.Compute(null).Should().Be(0);
        SimHash.Compute("   ").Should().Be(0);
    }
}
