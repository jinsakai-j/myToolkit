using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsintToolkit.Core.Entities;

namespace OsintToolkit.Infrastructure.Data.Configurations;

public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports");

        builder.HasKey(report => report.Id);

        builder.Property(report => report.Id).HasColumnName("id");
        builder.Property(report => report.ScanId).HasColumnName("scan_id").IsRequired();
        builder.Property(report => report.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
        builder.Property(report => report.FilePath).HasColumnName("file_path").HasMaxLength(1024).IsRequired();
        builder.Property(report => report.GeneratedAt).HasColumnName("generated_at").IsRequired();
    }
}

