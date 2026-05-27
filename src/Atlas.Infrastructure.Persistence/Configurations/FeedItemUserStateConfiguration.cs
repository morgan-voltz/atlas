using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class FeedItemUserStateConfiguration : IEntityTypeConfiguration<FeedItemUserState>
{
    public void Configure(EntityTypeBuilder<FeedItemUserState> builder)
    {
        builder.ToTable("feed_item_user_state");

        builder.HasKey(state => state.Id);

        builder.Property(state => state.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new FeedItemUserStateId(value))
            .ValueGeneratedNever();

        builder.Property(state => state.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        builder.Property(state => state.FeedItemId)
            .HasColumnName("feed_item_id")
            .HasConversion(id => id.Value, value => new FeedItemId(value))
            .IsRequired();

        builder.Property(state => state.IsRead).HasColumnName("is_read");
        builder.Property(state => state.IsFavorite).HasColumnName("is_favorite");
        builder.Property(state => state.IsArchived).HasColumnName("is_archived");
        builder.Property(state => state.UpdatedAt).HasColumnName("updated_at");

        // Un seul état par (utilisateur, item).
        builder.HasIndex(state => new { state.UserId, state.FeedItemId }).IsUnique();
        // Accélère l'exclusion des archivés dans la timeline.
        builder.HasIndex(state => new { state.UserId, state.IsArchived });

        builder.HasOne<FeedItem>()
            .WithMany()
            .HasForeignKey(state => state.FeedItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cascade RGPD (art. 17) : la suppression de l'utilisateur supprime ses états d'items.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(state => state.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(state => state.DomainEvents);
    }
}
