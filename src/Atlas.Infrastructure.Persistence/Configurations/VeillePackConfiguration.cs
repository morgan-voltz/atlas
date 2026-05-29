using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class VeillePackConfiguration : IEntityTypeConfiguration<VeillePack>
{
    public void Configure(EntityTypeBuilder<VeillePack> builder)
    {
        builder.ToTable("veille_pack");

        builder.HasKey(pack => pack.Id);

        builder.Property(pack => pack.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new VeillePackId(value))
            .ValueGeneratedNever();

        builder.Property(pack => pack.Code)
            .HasColumnName("code")
            .HasMaxLength(VeillePack.MaxCodeLength)
            .IsRequired();

        builder.Property(pack => pack.Name)
            .HasColumnName("name")
            .HasMaxLength(VeillePack.MaxNameLength)
            .IsRequired();

        builder.Property(pack => pack.Description)
            .HasColumnName("description")
            .HasMaxLength(VeillePack.MaxDescriptionLength)
            .IsRequired();

        builder.Property(pack => pack.Version).HasColumnName("version");
        builder.Property(pack => pack.IsActive).HasColumnName("is_active");
        builder.Property(pack => pack.CreatedAt).HasColumnName("created_at");

        // F-049 marketplace : auteur (nullable pour les packs système), visibilité, compteur de likes.
        builder.Property(pack => pack.AuthorUserId)
            .HasColumnName("author_user_id")
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null);

        builder.Property(pack => pack.Visibility)
            .HasColumnName("visibility")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(pack => pack.LikesCount)
            .HasColumnName("likes_count")
            .HasDefaultValue(0);

        builder.HasIndex(pack => pack.Code).IsUnique();
        builder.HasIndex(pack => new { pack.Visibility, pack.IsActive });

        // F-049 — cascade RGPD sur l'auteur (suppression du compte → suppression de ses packs).
        // Les packs système (author_user_id IS NULL) ne sont jamais affectés.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(pack => pack.AuthorUserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // Sources possédées par le pack (table de liaison vers feed_source via FeedSourceId).
        builder.OwnsMany(pack => pack.Items, items =>
        {
            items.ToTable("veille_pack_item");
            items.WithOwner().HasForeignKey("pack_id");

            items.Property(item => item.SourceId)
                .HasColumnName("source_id")
                .HasConversion(id => id.Value, value => new FeedSourceId(value));

            items.HasKey("pack_id", "SourceId");
        });

        builder.Navigation(pack => pack.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(pack => pack.DomainEvents);
    }
}
