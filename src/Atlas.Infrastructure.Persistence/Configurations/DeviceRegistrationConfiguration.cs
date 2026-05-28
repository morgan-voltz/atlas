using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class DeviceRegistrationConfiguration : IEntityTypeConfiguration<DeviceRegistration>
{
    public void Configure(EntityTypeBuilder<DeviceRegistration> builder)
    {
        builder.ToTable("device_registrations");

        builder.HasKey(device => device.Id);

        builder.Property(device => device.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new DeviceRegistrationId(value))
            .ValueGeneratedNever();

        builder.Property(device => device.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(device => device.Platform)
            .HasColumnName("platform")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(device => device.Token)
            .HasColumnName("token")
            .HasMaxLength(DeviceRegistration.MaxTokenLength)
            .IsRequired();

        builder.Property(device => device.Label)
            .HasColumnName("label")
            .HasMaxLength(DeviceRegistration.MaxLabelLength);

        builder.Property(device => device.RegisteredAt).HasColumnName("registered_at");
        builder.Property(device => device.LastSeenAt).HasColumnName("last_seen_at");

        // Le token est unique globalement (deux users distincts ne peuvent pas avoir le même token FCM/APNs).
        builder.HasIndex(device => device.Token).IsUnique();
        builder.HasIndex(device => device.UserId);

        // Cascade RGPD.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(device => device.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(device => device.DomainEvents);
    }
}
