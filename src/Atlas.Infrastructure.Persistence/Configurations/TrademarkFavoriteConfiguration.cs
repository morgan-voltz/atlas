using Atlas.Domain.Favorites;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class TrademarkFavoriteConfiguration : IEntityTypeConfiguration<TrademarkFavorite>
{
    public void Configure(EntityTypeBuilder<TrademarkFavorite> builder)
    {
        builder.ToTable("trademark_favorites");

        builder.HasKey(favorite => favorite.Id);

        builder.Property(favorite => favorite.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new TrademarkFavoriteId(value))
            .ValueGeneratedNever();

        builder.Property(favorite => favorite.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(favorite => favorite.DepositNumber)
            .HasColumnName("deposit_number")
            .HasConversion(d => d.Value, value => new DepositNumber(value))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(favorite => favorite.NameSnapshot)
            .HasColumnName("name_snapshot")
            .HasMaxLength(TrademarkFavorite.MaxNameSnapshotLength);

        builder.Property(favorite => favorite.AddedAt).HasColumnName("added_at");

        builder.HasIndex(favorite => new { favorite.UserId, favorite.DepositNumber }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(favorite => favorite.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(favorite => favorite.DomainEvents);
    }
}
