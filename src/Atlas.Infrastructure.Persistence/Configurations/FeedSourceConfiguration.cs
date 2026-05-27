using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class FeedSourceConfiguration : IEntityTypeConfiguration<FeedSource>
{
    public void Configure(EntityTypeBuilder<FeedSource> builder)
    {
        builder.ToTable("feed_source");

        builder.HasKey(source => source.Id);

        builder.Property(source => source.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new FeedSourceId(value))
            .ValueGeneratedNever();

        builder.Property(source => source.Name)
            .HasColumnName("name")
            .HasMaxLength(FeedSource.MaxNameLength)
            .IsRequired();

        builder.Property(source => source.Url)
            .HasColumnName("url")
            .HasMaxLength(FeedSource.MaxUrlLength)
            .IsRequired();

        builder.Property(source => source.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(source => source.PollingInterval).HasColumnName("polling_interval");
        builder.Property(source => source.IsActive).HasColumnName("is_active");
        builder.Property(source => source.CreatedAt).HasColumnName("created_at");
        builder.Property(source => source.LastPolledAt).HasColumnName("last_polled_at");

        builder.HasIndex(source => source.Url).IsUnique();

        builder.Ignore(source => source.DomainEvents);
    }
}
