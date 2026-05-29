using Atlas.Domain.Companies;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class FeedRuleConfiguration : IEntityTypeConfiguration<FeedRule>
{
    public void Configure(EntityTypeBuilder<FeedRule> builder)
    {
        builder.ToTable("feed_rule");

        builder.HasKey(rule => rule.Id);

        builder.Property(rule => rule.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new FeedRuleId(value))
            .ValueGeneratedNever();

        builder.Property(rule => rule.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(rule => rule.Name)
            .HasColumnName("name")
            .HasMaxLength(FeedRule.MaxNameLength)
            .IsRequired();

        builder.Property(rule => rule.KeywordPattern)
            .HasColumnName("keyword_pattern")
            .HasMaxLength(FeedRule.MaxKeywordLength);

        builder.Property(rule => rule.SourceId)
            .HasColumnName("source_id")
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new FeedSourceId(value.Value) : null);

        builder.Property(rule => rule.MentionedSiren)
            .HasColumnName("mentioned_siren")
            .HasMaxLength(9)
            .HasConversion(
                siren => siren != null ? siren.Value.Value : null,
                value => value != null ? Siren.FromTrustedValue(value) : null);

        builder.Property(rule => rule.NotifyEmail).HasColumnName("notify_email");
        builder.Property(rule => rule.NotifyPush).HasColumnName("notify_push");
        builder.Property(rule => rule.IsActive).HasColumnName("is_active");
        builder.Property(rule => rule.CreatedAt).HasColumnName("created_at");
        builder.Property(rule => rule.LastEvaluatedAt).HasColumnName("last_evaluated_at");
        builder.Property(rule => rule.LastTriggeredAt).HasColumnName("last_triggered_at");
        builder.Property(rule => rule.TimesTriggered).HasColumnName("times_triggered");

        builder.HasIndex(rule => rule.UserId);
        builder.HasIndex(rule => rule.IsActive);

        // Cascade RGPD (art. 17) : suppression du compte → suppression des règles.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(rule => rule.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(rule => rule.DomainEvents);
        builder.Ignore(rule => rule.EvaluationWatermark);
    }
}
