using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsintToolkit.Core.Entities;

namespace OsintToolkit.Infrastructure.Data.Configurations;

public sealed class ScanConfiguration : IEntityTypeConfiguration<Scan>
{
    public void Configure(EntityTypeBuilder<Scan> builder)
    {
        builder.ToTable("scans");

        builder.HasKey(scan => scan.Id);

        builder.Property(scan => scan.Id).HasColumnName("id");
        builder.Property(scan => scan.Target).HasColumnName("target").HasMaxLength(512).IsRequired();
        builder.Property(scan => scan.TargetType)
            .HasColumnName("target_type")
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(scan => scan.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(scan => scan.RiskScore).HasColumnName("risk_score");
        builder.Property(scan => scan.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(scan => scan.StartedAt).HasColumnName("started_at");
        builder.Property(scan => scan.CompletedAt).HasColumnName("completed_at");
        builder.Property(scan => scan.Notes).HasColumnName("notes");

        builder.HasMany(scan => scan.Results)
            .WithOne(result => result.Scan)
            .HasForeignKey(result => result.ScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(scan => scan.Reports)
            .WithOne(report => report.Scan)
            .HasForeignKey(report => report.ScanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
