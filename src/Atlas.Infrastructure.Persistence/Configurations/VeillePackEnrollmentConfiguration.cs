using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class VeillePackEnrollmentConfiguration : IEntityTypeConfiguration<VeillePackEnrollment>
{
    public void Configure(EntityTypeBuilder<VeillePackEnrollment> builder)
    {
        builder.ToTable("veille_pack_enrollment");

        builder.HasKey(enrollment => enrollment.Id);

        builder.Property(enrollment => enrollment.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new VeillePackEnrollmentId(value))
            .ValueGeneratedNever();

        builder.Property(enrollment => enrollment.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        builder.Property(enrollment => enrollment.PackId)
            .HasColumnName("pack_id")
            .HasConversion(id => id.Value, value => new VeillePackId(value))
            .IsRequired();

        builder.Property(enrollment => enrollment.AppliedVersion).HasColumnName("applied_version");
        builder.Property(enrollment => enrollment.CreatedAt).HasColumnName("created_at");
        builder.Property(enrollment => enrollment.UpdatedAt).HasColumnName("updated_at");

        // Un utilisateur ne s'inscrit qu'une fois à un pack donné.
        builder.HasIndex(enrollment => new { enrollment.UserId, enrollment.PackId }).IsUnique();

        builder.Ignore(enrollment => enrollment.DomainEvents);
    }
}
