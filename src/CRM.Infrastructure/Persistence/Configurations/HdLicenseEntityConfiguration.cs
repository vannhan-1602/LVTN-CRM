using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HdLicenseEntityConfiguration : IEntityTypeConfiguration<HdLicenseEntity>
{
    public void Configure(EntityTypeBuilder<HdLicenseEntity> b)
    {
        b.ToTable("HD_License");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.HopDong_Id).IsRequired();
        b.Property(x => x.SanPham_Id).IsRequired();
        b.Property(x => x.SoLuongUser).HasDefaultValue(1);
        b.Property(x => x.PhienBan).HasMaxLength(50);
        b.Property(x => x.MaLicenseKey).HasMaxLength(100);
        b.HasIndex(x => x.MaLicenseKey).IsUnique();
        b.Property(x => x.MoiTruongTrienKhai).HasMaxLength(20).HasDefaultValue("Cloud");
        b.Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("DangHoatDong");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        b.HasOne(x => x.HopDong)
         .WithMany()
         .HasForeignKey(x => x.HopDong_Id)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.SanPham)
         .WithMany()
         .HasForeignKey(x => x.SanPham_Id)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
