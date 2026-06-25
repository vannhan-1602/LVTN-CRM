using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class KhKhachHangEntityConfiguration : IEntityTypeConfiguration<KhKhachHangEntity>
{
    public void Configure(EntityTypeBuilder<KhKhachHangEntity> builder)
    {
        builder.ToTable("KH_KhachHang");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.MaKhachHang)
            .HasColumnName("MaKhachHang")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.TenKhachHang)
            .HasColumnName("TenKhachHang")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LoaiKhachHangId)
            .HasColumnName("LoaiKhachHang_Id");

        builder.Property(e => e.TinhTrangId)
            .HasColumnName("TinhTrang_Id");

        builder.Property(e => e.Email)
            .HasColumnName("Email")
            .HasMaxLength(100);

        builder.Property(e => e.SoDienThoai)
            .HasColumnName("SoDienThoai")
            .HasMaxLength(20);

        builder.Property(e => e.MaSoThue)
            .HasColumnName("MaSoThue")
            .HasMaxLength(50);

        builder.Property(e => e.NhanVienPhuTrachId)
            .HasColumnName("NhanVienPhuTrach_Id");

        builder.Property(e => e.IsDeleted)
            .HasColumnName("IsDeleted");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("UpdatedAt");

        builder.HasIndex(e => e.MaKhachHang)
            .IsUnique();
    }
}