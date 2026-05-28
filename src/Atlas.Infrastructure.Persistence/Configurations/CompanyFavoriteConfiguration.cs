using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class CompanyFavoriteConfiguration : IEntityTypeConfiguration<CompanyFavorite>
{
    public void Configure(EntityTypeBuilder<CompanyFavorite> builder)
    {
        builder.ToTable("company_favorites");

        builder.HasKey(favorite => favorite.Id);

        builder.Property(favorite => favorite.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new CompanyFavoriteId(value))
            .ValueGeneratedNever();

        builder.Property(favorite => favorite.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(favorite => favorite.Siren)
            .HasColumnName("siren")
            .HasConversion(siren => siren.Value, value => Siren.FromTrustedValue(value))
            .HasMaxLength(9)
            .IsRequired();

        builder.Property(favorite => favorite.NameSnapshot)
            .HasColumnName("name_snapshot")
            .HasMaxLength(CompanyFavorite.MaxNameSnapshotLength);

        builder.Property(favorite => favorite.AddedAt).HasColumnName("added_at");

        // Un utilisateur ne peut pas marquer deux fois la même entreprise comme favorite.
        builder.HasIndex(favorite => new { favorite.UserId, favorite.Siren }).IsUnique();

        // Cascade RGPD (art. 17) : suppression du compte → suppression des favoris.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(favorite => favorite.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(favorite => favorite.DomainEvents);
    }
}
