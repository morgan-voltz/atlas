using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class CompanyFavoriteSnapshotConfiguration : IEntityTypeConfiguration<CompanyFavoriteSnapshot>
{
    public void Configure(EntityTypeBuilder<CompanyFavoriteSnapshot> builder)
    {
        builder.ToTable("company_favorite_snapshots");

        builder.HasKey(snap => snap.Id);

        builder.Property(snap => snap.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new CompanyFavoriteSnapshotId(value))
            .ValueGeneratedNever();

        builder.Property(snap => snap.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(snap => snap.Siren)
            .HasColumnName("siren")
            .HasConversion(siren => siren.Value, value => Siren.FromTrustedValue(value))
            .HasMaxLength(9)
            .IsRequired();

        builder.Property(snap => snap.Denomination)
            .HasColumnName("denomination")
            .HasMaxLength(CompanyFavoriteSnapshot.MaxFieldLength);

        builder.Property(snap => snap.FormeJuridique)
            .HasColumnName("forme_juridique")
            .HasMaxLength(CompanyFavoriteSnapshot.MaxFieldLength);

        builder.Property(snap => snap.NafCode)
            .HasColumnName("naf_code")
            .HasMaxLength(CompanyFavoriteSnapshot.MaxFieldLength);

        builder.Property(snap => snap.AdresseLine)
            .HasColumnName("adresse_line")
            .HasMaxLength(CompanyFavoriteSnapshot.MaxFieldLength);

        builder.Property(snap => snap.DirigeantsHash)
            .HasColumnName("dirigeants_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(snap => snap.CapturedAt).HasColumnName("captured_at");

        // Un seul snapshot vivant par (user, siren) — politique « replace ».
        builder.HasIndex(snap => new { snap.UserId, snap.Siren }).IsUnique();

        // Cascade RGPD (art. 17).
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(snap => snap.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(snap => snap.DomainEvents);
    }
}
