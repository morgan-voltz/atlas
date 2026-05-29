using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class VeillePackLikeConfiguration : IEntityTypeConfiguration<VeillePackLike>
{
    public void Configure(EntityTypeBuilder<VeillePackLike> builder)
    {
        builder.ToTable("veille_pack_like");

        builder.HasKey(like => like.Id);

        builder.Property(like => like.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new VeillePackLikeId(value))
            .ValueGeneratedNever();

        builder.Property(like => like.VeillePackId)
            .HasColumnName("veille_pack_id")
            .HasConversion(id => id.Value, value => new VeillePackId(value));

        builder.Property(like => like.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(like => like.CreatedAt).HasColumnName("created_at");

        // Un user ne peut liker qu'une fois par pack.
        builder.HasIndex(like => new { like.UserId, like.VeillePackId }).IsUnique();
        builder.HasIndex(like => like.VeillePackId);

        // Cascade RGPD sur l'utilisateur.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(like => like.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cascade sur le pack : si le pack est supprimé, ses likes le sont aussi.
        builder.HasOne<VeillePack>()
            .WithMany()
            .HasForeignKey(like => like.VeillePackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(like => like.DomainEvents);
    }
}
