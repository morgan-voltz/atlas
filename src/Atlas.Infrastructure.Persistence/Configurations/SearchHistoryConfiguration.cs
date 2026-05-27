using Atlas.Domain.Search;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class SearchHistoryConfiguration : IEntityTypeConfiguration<SearchHistoryEntry>
{
    public void Configure(EntityTypeBuilder<SearchHistoryEntry> builder)
    {
        builder.ToTable("search_history");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new SearchHistoryEntryId(value))
            .ValueGeneratedNever();

        builder.Property(entry => entry.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(entry => entry.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(entry => entry.Query)
            .HasColumnName("query")
            .HasMaxLength(SearchHistoryEntry.MaxQueryLength)
            .IsRequired();

        builder.Property(entry => entry.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(entry => new { entry.UserId, entry.CreatedAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(entry => entry.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(entry => entry.DomainEvents);
    }
}
