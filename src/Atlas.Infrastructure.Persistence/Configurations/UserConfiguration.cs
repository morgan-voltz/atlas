using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new UserId(value))
            .ValueGeneratedNever();

        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasConversion(email => email.Value, value => EmailAddress.FromStorage(value))
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(user => user.Email).IsUnique();

        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash")
            .HasConversion(hash => hash.Value, value => PasswordHash.FromHash(value))
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(user => user.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(user => user.CreatedAt).HasColumnName("created_at");
        builder.Property(user => user.EmailVerifiedAt).HasColumnName("email_verified_at");
        builder.Property(user => user.FailedLoginAttempts).HasColumnName("failed_login_attempts");
        builder.Property(user => user.LockoutEndsAt).HasColumnName("lockout_ends_at");

        builder.Property<string?>("_emailVerificationTokenHash")
            .HasColumnName("email_verification_token_hash")
            .HasMaxLength(128);

        builder.Property<DateTimeOffset?>("_emailVerificationTokenExpiresAt")
            .HasColumnName("email_verification_token_expires_at");

        builder.Property<string?>("_passwordResetTokenHash")
            .HasColumnName("password_reset_token_hash")
            .HasMaxLength(128);

        builder.Property<DateTimeOffset?>("_passwordResetTokenExpiresAt")
            .HasColumnName("password_reset_token_expires_at");

        builder.Property(user => user.TwoFactorEnabled).HasColumnName("two_factor_enabled");

        builder.Property(user => user.TwoFactorSecret)
            .HasColumnName("two_factor_secret")
            .HasMaxLength(512);

        builder.Property(user => user.PendingTwoFactorSecret)
            .HasColumnName("pending_two_factor_secret")
            .HasMaxLength(512);

        builder.OwnsOne(user => user.AccessibilityPreferences, prefs =>
        {
            prefs.Property(p => p.HighContrast)
                .HasColumnName("a11y_high_contrast")
                .HasDefaultValue(false)
                .IsRequired();

            prefs.Property(p => p.ReduceMotion)
                .HasColumnName("a11y_reduce_motion")
                .HasDefaultValue(false)
                .IsRequired();

            prefs.Property(p => p.FontPreference)
                .HasColumnName("a11y_font_preference")
                .HasConversion<string>()
                .HasMaxLength(32)
                .HasDefaultValue(AccessibilityFontPreference.Default)
                .IsRequired();
        });

        builder.Navigation(user => user.AccessibilityPreferences).IsRequired();

        builder.Ignore(user => user.DomainEvents);
    }
}
