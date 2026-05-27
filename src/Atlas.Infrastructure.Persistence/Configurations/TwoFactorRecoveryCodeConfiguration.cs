using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class TwoFactorRecoveryCodeConfiguration : IEntityTypeConfiguration<TwoFactorRecoveryCode>
{
    public void Configure(EntityTypeBuilder<TwoFactorRecoveryCode> builder)
    {
        builder.ToTable("two_factor_recovery_codes");

        builder.HasKey(code => code.Id);

        builder.Property(code => code.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new RecoveryCodeId(value))
            .ValueGeneratedNever();

        builder.Property(code => code.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(code => code.CodeHash)
            .HasColumnName("code_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(code => code.CreatedAt).HasColumnName("created_at");
        builder.Property(code => code.UsedAt).HasColumnName("used_at");

        builder.HasIndex(code => new { code.UserId, code.CodeHash });

        builder.Ignore(code => code.DomainEvents);
    }
}
