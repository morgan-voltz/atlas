using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class VeillePackConfiguration : IEntityTypeConfiguration<VeillePack>
{
    public void Configure(EntityTypeBuilder<VeillePack> builder)
    {
        builder.ToTable("veille_pack");

        builder.HasKey(pack => pack.Id);

        builder.Property(pack => pack.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new VeillePackId(value))
            .ValueGeneratedNever();

        builder.Property(pack => pack.Code)
            .HasColumnName("code")
            .HasMaxLength(VeillePack.MaxCodeLength)
            .IsRequired();

        builder.Property(pack => pack.Name)
            .HasColumnName("name")
            .HasMaxLength(VeillePack.MaxNameLength)
            .IsRequired();

        builder.Property(pack => pack.Description)
            .HasColumnName("description")
            .HasMaxLength(VeillePack.MaxDescriptionLength)
            .IsRequired();

        builder.Property(pack => pack.Version).HasColumnName("version");
        builder.Property(pack => pack.IsActive).HasColumnName("is_active");
        builder.Property(pack => pack.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(pack => pack.Code).IsUnique();

        // Sources possédées par le pack (table de liaison vers feed_source via FeedSourceId).
        builder.OwnsMany(pack => pack.Items, items =>
        {
            items.ToTable("veille_pack_item");
            items.WithOwner().HasForeignKey("pack_id");

            items.Property(item => item.SourceId)
                .HasColumnName("source_id")
                .HasConversion(id => id.Value, value => new FeedSourceId(value));

            items.HasKey("pack_id", "SourceId");
        });

        builder.Navigation(pack => pack.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(pack => pack.DomainEvents);
    }
}
