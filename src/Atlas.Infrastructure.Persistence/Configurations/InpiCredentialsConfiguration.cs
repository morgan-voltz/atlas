using Atlas.Domain.Inpi;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class InpiCredentialsConfiguration : IEntityTypeConfiguration<InpiCredentials>
{
    public void Configure(EntityTypeBuilder<InpiCredentials> builder)
    {
        builder.ToTable("inpi_credentials");

        builder.HasKey(credentials => credentials.Id);

        builder.Property(credentials => credentials.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new InpiCredentialsId(value))
            .ValueGeneratedNever();

        builder.Property(credentials => credentials.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        // Relation 1-1 : un compte INPI connecté par utilisateur (cf. ADR-003).
        builder.HasIndex(credentials => credentials.UserId).IsUnique();

        builder.Property(credentials => credentials.EncryptedUsername)
            .HasColumnName("encrypted_username")
            .IsRequired();

        builder.Property(credentials => credentials.EncryptedPassword)
            .HasColumnName("encrypted_password")
            .IsRequired();

        builder.Property(credentials => credentials.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(credentials => credentials.CreatedAt).HasColumnName("created_at");
        builder.Property(credentials => credentials.UpdatedAt).HasColumnName("updated_at");
        builder.Property(credentials => credentials.LastTestedAt).HasColumnName("last_tested_at");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(credentials => credentials.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(credentials => credentials.DomainEvents);
    }
}
