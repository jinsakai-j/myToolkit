using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsintToolkit.Core.Entities;

namespace OsintToolkit.Infrastructure.Data.Configurations;

public sealed class ScanResultConfiguration : IEntityTypeConfiguration<ScanResult>
{
    public void Configure(EntityTypeBuilder<ScanResult> builder)
    {
        builder.ToTable("scan_results");

        builder.HasKey(result => result.Id);

        builder.Property(result => result.Id).HasColumnName("id");
        builder.Property(result => result.ScanId).HasColumnName("scan_id").IsRequired();
        builder.Property(result => result.ModuleName).HasColumnName("module_name").HasMaxLength(128).IsRequired();
        builder.Property(result => result.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(result => result.Summary).HasColumnName("summary");
        builder.Property(result => result.RawData).HasColumnName("raw_data").HasColumnType("jsonb");
        builder.Property(result => result.CreatedAt).HasColumnName("created_at").IsRequired();
    }
}
