using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class VeillePackReportConfiguration : IEntityTypeConfiguration<VeillePackReport>
{
    public void Configure(EntityTypeBuilder<VeillePackReport> builder)
    {
        builder.ToTable("veille_pack_report");

        builder.HasKey(report => report.Id);

        builder.Property(report => report.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new VeillePackReportId(value))
            .ValueGeneratedNever();

        builder.Property(report => report.VeillePackId)
            .HasColumnName("veille_pack_id")
            .HasConversion(id => id.Value, value => new VeillePackId(value));

        builder.Property(report => report.ReporterUserId)
            .HasColumnName("reporter_user_id")
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.Property(report => report.Reason)
            .HasColumnName("reason")
            .HasMaxLength(VeillePackReport.MaxReasonLength)
            .IsRequired();

        builder.Property(report => report.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(report => report.CreatedAt).HasColumnName("created_at");
        builder.Property(report => report.ReviewedAt).HasColumnName("reviewed_at");

        builder.HasIndex(report => new { report.VeillePackId, report.Status });
        builder.HasIndex(report => new { report.ReporterUserId, report.VeillePackId, report.Status });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(report => report.ReporterUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<VeillePack>()
            .WithMany()
            .HasForeignKey(report => report.VeillePackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(report => report.DomainEvents);
    }
}
