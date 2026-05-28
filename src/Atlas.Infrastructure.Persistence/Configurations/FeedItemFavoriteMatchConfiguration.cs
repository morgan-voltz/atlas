using Atlas.Domain.Companies;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class FeedItemFavoriteMatchConfiguration : IEntityTypeConfiguration<FeedItemFavoriteMatch>
{
    public void Configure(EntityTypeBuilder<FeedItemFavoriteMatch> builder)
    {
        builder.ToTable("feed_item_favorite_matches");

        builder.HasKey(match => match.Id);

        builder.Property(match => match.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new FeedItemFavoriteMatchId(value))
            .ValueGeneratedNever();

        builder.Property(match => match.FeedItemId)
            .HasColumnName("feed_item_id")
            .HasConversion(id => id.Value, value => new FeedItemId(value));

        builder.Property(match => match.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(match => match.Siren)
            .HasColumnName("siren")
            .HasConversion(siren => siren.Value, value => Siren.FromTrustedValue(value))
            .HasMaxLength(9)
            .IsRequired();

        builder.Property(match => match.MatchedName)
            .HasColumnName("matched_name")
            .HasMaxLength(FeedItemFavoriteMatch.MaxMatchedNameLength)
            .IsRequired();

        builder.Property(match => match.MatchedAt).HasColumnName("matched_at");

        // Une mention par (user, item, siren) — on évite les doublons à l'insertion.
        builder.HasIndex(match => new { match.UserId, match.FeedItemId, match.Siren }).IsUnique();
        builder.HasIndex(match => match.FeedItemId);

        // Cascade RGPD sur User.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(match => match.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cascade sur FeedItem : si l'item est supprimé, les mentions le sont aussi.
        builder.HasOne<FeedItem>()
            .WithMany()
            .HasForeignKey(match => match.FeedItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(match => match.DomainEvents);
    }
}
