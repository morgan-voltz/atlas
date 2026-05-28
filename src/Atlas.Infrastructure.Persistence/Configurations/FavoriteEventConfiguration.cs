using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class FavoriteEventConfiguration : IEntityTypeConfiguration<FavoriteEvent>
{
    public void Configure(EntityTypeBuilder<FavoriteEvent> builder)
    {
        builder.ToTable("favorite_events");

        builder.HasKey(evt => evt.Id);

        builder.Property(evt => evt.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new FavoriteEventId(value))
            .ValueGeneratedNever();

        builder.Property(evt => evt.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(evt => evt.Siren)
            .HasColumnName("siren")
            .HasConversion(siren => siren.Value, value => Siren.FromTrustedValue(value))
            .HasMaxLength(9)
            .IsRequired();

        builder.Property(evt => evt.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(evt => evt.Title)
            .HasColumnName("title")
            .HasMaxLength(FavoriteEvent.MaxTitleLength)
            .IsRequired();

        builder.Property(evt => evt.Summary)
            .HasColumnName("summary")
            .HasMaxLength(FavoriteEvent.MaxSummaryLength);

        builder.Property(evt => evt.OccurredAt).HasColumnName("occurred_at");

        builder.Property(evt => evt.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(FavoriteEvent.MaxExternalIdLength);

        builder.HasIndex(evt => new { evt.UserId, evt.OccurredAt });

        // F-048 : dédup BODACC. Index unique partiel sur (user_id, external_id) où external_id IS NOT NULL.
        builder.HasIndex(evt => new { evt.UserId, evt.ExternalId })
            .IsUnique()
            .HasFilter("external_id IS NOT NULL");

        // Cascade RGPD (art. 17).
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(evt => evt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(evt => evt.DomainEvents);
    }
}
