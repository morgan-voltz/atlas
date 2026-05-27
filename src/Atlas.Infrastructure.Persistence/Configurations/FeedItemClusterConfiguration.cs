using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class FeedItemClusterConfiguration : IEntityTypeConfiguration<FeedItemCluster>
{
    public void Configure(EntityTypeBuilder<FeedItemCluster> builder)
    {
        builder.ToTable("feed_item_cluster");

        builder.HasKey(cluster => cluster.Id);

        builder.Property(cluster => cluster.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new FeedItemClusterId(value))
            .ValueGeneratedNever();

        builder.Property(cluster => cluster.SimHash).HasColumnName("simhash");

        builder.Property(cluster => cluster.ItemCount).HasColumnName("item_count");

        builder.Property(cluster => cluster.FirstPublishedAt).HasColumnName("first_published_at");

        builder.Property(cluster => cluster.LastPublishedAt).HasColumnName("last_published_at");

        builder.Property(cluster => cluster.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(cluster => cluster.LastPublishedAt);

        builder.Ignore(cluster => cluster.DomainEvents);
    }
}
