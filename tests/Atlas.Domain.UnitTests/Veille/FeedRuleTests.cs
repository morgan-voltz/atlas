using Atlas.Domain.Companies;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Veille;

public sealed class FeedRuleTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 14, 0, 0, TimeSpan.Zero);
    private static readonly UserId User = new(Guid.NewGuid());
    private static readonly FeedSourceId Source = FeedSourceId.New();
    private static readonly Siren ConcurrentSiren = Siren.Create("552120222").Value!;

    [Fact]
    public void Create_with_keyword_only_succeeds()
    {
        var result = FeedRule.Create(User, "Mes mentions RGPD", "RGPD", null, null, true, true, Now);

        result.IsSuccess.Should().BeTrue();
        FeedRule rule = result.Value!;
        rule.IsActive.Should().BeTrue();
        rule.KeywordPattern.Should().Be("RGPD");
        rule.NotifyEmail.Should().BeTrue();
        rule.NotifyPush.Should().BeTrue();
        rule.TimesTriggered.Should().Be(0);
        rule.LastTriggeredAt.Should().BeNull();
        rule.LastEvaluatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_with_no_criteria_fails()
    {
        var result = FeedRule.Create(User, "Vide", null, null, null, true, false, Now);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.invalid_feed_rule");
    }

    [Fact]
    public void Create_with_no_action_fails()
    {
        var result = FeedRule.Create(User, "Sans action", "RGPD", null, null, false, false, Now);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.invalid_feed_rule");
    }

    [Fact]
    public void Create_with_empty_name_fails()
    {
        var result = FeedRule.Create(User, "   ", "RGPD", null, null, true, false, Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_with_too_long_name_fails()
    {
        string longName = new('A', FeedRule.MaxNameLength + 1);

        var result = FeedRule.Create(User, longName, "RGPD", null, null, true, false, Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_with_too_long_keyword_fails()
    {
        string longKeyword = new('K', FeedRule.MaxKeywordLength + 1);

        var result = FeedRule.Create(User, "OK", longKeyword, null, null, true, false, Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Matches_keyword_in_title_returns_true()
    {
        FeedRule rule = FeedRule.Create(User, "Mention RGPD", "RGPD", null, null, true, false, Now).Value!;
        FeedItem item = FeedItem.Create(
            Source, "Le RGPD évolue en 2026", "https://x", "Détail…", Now, null, Now);

        rule.Matches(item, []).Should().BeTrue();
    }

    [Fact]
    public void Matches_keyword_in_summary_returns_true()
    {
        FeedRule rule = FeedRule.Create(User, "Mention RGPD", "rgpd", null, null, true, false, Now).Value!;
        FeedItem item = FeedItem.Create(
            Source, "Actualité juridique", "https://x", "Le RGPD a évolué.", Now, null, Now);

        rule.Matches(item, []).Should().BeTrue();
    }

    [Fact]
    public void Matches_keyword_not_found_returns_false()
    {
        FeedRule rule = FeedRule.Create(User, "Mention RGPD", "RGPD", null, null, true, false, Now).Value!;
        FeedItem item = FeedItem.Create(
            Source, "Autre sujet", "https://x", "Aucun lien.", Now, null, Now);

        rule.Matches(item, []).Should().BeFalse();
    }

    [Fact]
    public void Matches_wrong_source_returns_false_when_source_constrained()
    {
        FeedRule rule = FeedRule.Create(User, "Source", null, Source, null, true, false, Now).Value!;
        FeedItem item = FeedItem.Create(
            FeedSourceId.New(), "Titre", "https://x", null, Now, null, Now);

        rule.Matches(item, []).Should().BeFalse();
    }

    [Fact]
    public void Matches_inactive_returns_false()
    {
        FeedRule rule = FeedRule.Create(User, "RGPD", "RGPD", null, null, true, false, Now).Value!;
        rule.Update("RGPD", "RGPD", null, null, true, false, isActive: false);
        FeedItem item = FeedItem.Create(Source, "RGPD", "https://x", null, Now, null, Now);

        rule.Matches(item, []).Should().BeFalse();
    }

    [Fact]
    public void Matches_mentioned_siren_present_returns_true()
    {
        FeedRule rule = FeedRule.Create(User, "Suivi concurrent", null, null, ConcurrentSiren, true, false, Now).Value!;
        FeedItem item = FeedItem.Create(Source, "Titre", "https://x", null, Now, null, Now);

        rule.Matches(item, [ConcurrentSiren]).Should().BeTrue();
    }

    [Fact]
    public void Matches_mentioned_siren_absent_returns_false()
    {
        FeedRule rule = FeedRule.Create(User, "Suivi concurrent", null, null, ConcurrentSiren, true, false, Now).Value!;
        FeedItem item = FeedItem.Create(Source, "Titre", "https://x", null, Now, null, Now);

        rule.Matches(item, []).Should().BeFalse();
    }

    [Fact]
    public void Matches_combined_criteria_AND_semantics()
    {
        FeedRule rule = FeedRule.Create(User, "Combo", "RGPD", Source, null, true, false, Now).Value!;

        FeedItem matchBoth = FeedItem.Create(Source, "RGPD 2026", "https://x", null, Now, null, Now);
        FeedItem matchKeywordWrongSource = FeedItem.Create(FeedSourceId.New(), "RGPD 2026", "https://x", null, Now, null, Now);
        FeedItem matchSourceWrongKeyword = FeedItem.Create(Source, "Autre", "https://x", null, Now, null, Now);

        rule.Matches(matchBoth, []).Should().BeTrue();
        rule.Matches(matchKeywordWrongSource, []).Should().BeFalse();
        rule.Matches(matchSourceWrongKeyword, []).Should().BeFalse();
    }

    [Fact]
    public void RegisterTrigger_increments_count_and_sets_timestamp()
    {
        FeedRule rule = FeedRule.Create(User, "RGPD", "RGPD", null, null, true, false, Now).Value!;
        DateTimeOffset later = Now.AddHours(1);

        rule.RegisterTrigger(later);
        rule.RegisterTrigger(later.AddMinutes(5));

        rule.TimesTriggered.Should().Be(2);
        rule.LastTriggeredAt.Should().Be(later.AddMinutes(5));
    }

    [Fact]
    public void RegisterEvaluation_updates_watermark()
    {
        FeedRule rule = FeedRule.Create(User, "RGPD", "RGPD", null, null, true, false, Now).Value!;
        DateTimeOffset later = Now.AddMinutes(30);

        rule.EvaluationWatermark.Should().Be(Now);
        rule.RegisterEvaluation(later);
        rule.EvaluationWatermark.Should().Be(later);
    }

    [Fact]
    public void Update_with_new_keyword_succeeds()
    {
        FeedRule rule = FeedRule.Create(User, "v1", "RGPD", null, null, true, false, Now).Value!;

        Result updated = rule.Update("v2", "CSRD", null, null, false, true, isActive: true);

        updated.IsSuccess.Should().BeTrue();
        rule.Name.Should().Be("v2");
        rule.KeywordPattern.Should().Be("CSRD");
        rule.NotifyEmail.Should().BeFalse();
        rule.NotifyPush.Should().BeTrue();
    }
}
