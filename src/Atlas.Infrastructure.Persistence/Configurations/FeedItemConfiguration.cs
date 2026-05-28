using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class FeedItemConfiguration : IEntityTypeConfiguration<FeedItem>
{
    public void Configure(EntityTypeBuilder<FeedItem> builder)
    {
        builder.ToTable("feed_item");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new FeedItemId(value))
            .ValueGeneratedNever();

        builder.Property(item => item.SourceId)
            .HasColumnName("source_id")
            .HasConversion(id => id.Value, value => new FeedSourceId(value));

        builder.Property(item => item.Title)
            .HasColumnName("title")
            .HasMaxLength(FeedItem.MaxTitleLength)
            .IsRequired();

        builder.Property(item => item.Url)
            .HasColumnName("url")
            .HasMaxLength(FeedItem.MaxUrlLength);

        builder.Property(item => item.Summary)
            .HasColumnName("summary")
            .HasMaxLength(FeedItem.MaxSummaryLength);

        builder.Property(item => item.PublishedAt).HasColumnName("published_at");

        builder.Property(item => item.Categories)
            .HasColumnName("categories")
            .HasMaxLength(FeedItem.MaxCategoriesLength);

        builder.Property(item => item.ContentHash)
            .HasColumnName("content_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(item => item.FetchedAt).HasColumnName("fetched_at");

        builder.Property(item => item.ClusterId)
            .HasColumnName("cluster_id")
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value.Value,
                value => value == null ? (FeedItemClusterId?)null : new FeedItemClusterId(value.Value));

        // Déduplication : un même hash ne peut exister qu'une fois par source.
        builder.HasIndex(item => new { item.SourceId, item.ContentHash }).IsUnique();
        builder.HasIndex(item => item.PublishedAt);
        builder.HasIndex(item => item.ClusterId);

        builder.HasOne<FeedSource>()
            .WithMany()
            .HasForeignKey(item => item.SourceId)
            .OnDelete(DeleteBehavior.Cascade);

        // F-045 : le cluster de déduplication ; sa suppression détache l'item sans le supprimer.
        builder.HasOne<FeedItemCluster>()
            .WithMany()
            .HasForeignKey(item => item.ClusterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(item => item.DomainEvents);
    }
}
