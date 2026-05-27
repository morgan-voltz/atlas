using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class VeilleSubscriptionConfiguration : IEntityTypeConfiguration<VeilleSubscription>
{
    public void Configure(EntityTypeBuilder<VeilleSubscription> builder)
    {
        builder.ToTable("veille_subscription");

        builder.HasKey(subscription => subscription.Id);

        builder.Property(subscription => subscription.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new VeilleSubscriptionId(value))
            .ValueGeneratedNever();

        builder.Property(subscription => subscription.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        builder.Property(subscription => subscription.SourceId)
            .HasColumnName("source_id")
            .HasConversion(id => id.Value, value => new FeedSourceId(value))
            .IsRequired();

        builder.Property(subscription => subscription.CreatedAt).HasColumnName("created_at");

        // Un utilisateur ne s'abonne qu'une fois à une même source.
        builder.HasIndex(subscription => new { subscription.UserId, subscription.SourceId }).IsUnique();

        builder.HasIndex(subscription => subscription.SourceId);

        builder.Ignore(subscription => subscription.DomainEvents);
    }
}
