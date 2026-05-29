using Atlas.Domain.Favorites;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class PatentFavoriteConfiguration : IEntityTypeConfiguration<PatentFavorite>
{
    public void Configure(EntityTypeBuilder<PatentFavorite> builder)
    {
        builder.ToTable("patent_favorites");

        builder.HasKey(favorite => favorite.Id);

        builder.Property(favorite => favorite.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new PatentFavoriteId(value))
            .ValueGeneratedNever();

        builder.Property(favorite => favorite.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(favorite => favorite.PublicationNumber)
            .HasColumnName("publication_number")
            .HasConversion(p => p.Value, value => PublicationNumber.FromTrustedValue(value))
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(favorite => favorite.TitleSnapshot)
            .HasColumnName("title_snapshot")
            .HasMaxLength(PatentFavorite.MaxTitleSnapshotLength);

        builder.Property(favorite => favorite.AddedAt).HasColumnName("added_at");

        builder.HasIndex(favorite => new { favorite.UserId, favorite.PublicationNumber }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(favorite => favorite.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(favorite => favorite.DomainEvents);
    }
}
