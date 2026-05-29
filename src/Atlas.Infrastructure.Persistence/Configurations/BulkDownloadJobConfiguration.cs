using Atlas.Domain.Downloads;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class BulkDownloadJobConfiguration : IEntityTypeConfiguration<BulkDownloadJob>
{
    public void Configure(EntityTypeBuilder<BulkDownloadJob> builder)
    {
        builder.ToTable("bulk_download_jobs");

        builder.HasKey(job => job.Id);

        builder.Property(job => job.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new BulkDownloadJobId(value))
            .ValueGeneratedNever();

        builder.Property(job => job.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(job => job.Sirens)
            .HasColumnName("sirens")
            .HasMaxLength(BulkDownloadJob.SirensColumnMaxLength)
            .IsRequired();

        builder.Property(job => job.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(job => job.ArchiveKey)
            .HasColumnName("archive_key")
            .HasMaxLength(128);

        builder.Property(job => job.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(BulkDownloadJob.MaxErrorMessageLength);

        builder.Property(job => job.RequestedAt).HasColumnName("requested_at");
        builder.Property(job => job.CompletedAt).HasColumnName("completed_at");
        builder.Property(job => job.ExpiresAt).HasColumnName("expires_at");

        builder.HasIndex(job => new { job.UserId, job.RequestedAt });

        // Cascade RGPD (art. 17).
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(job => job.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(job => job.DomainEvents);
    }
}
