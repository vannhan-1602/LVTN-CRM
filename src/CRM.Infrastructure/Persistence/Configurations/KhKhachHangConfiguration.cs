using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class KhKhachHangConfiguration : IEntityTypeConfiguration<KhKhachHang>
{
    public void Configure(EntityTypeBuilder<KhKhachHang> builder)
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

public class KhLoaiKhachHangConfiguration : IEntityTypeConfiguration<KhLoaiKhachHang>
{
    public void Configure(EntityTypeBuilder<KhLoaiKhachHang> builder)
    {
        builder.ToTable("KH_LoaiKhachHang");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id");
        builder.Property(e => e.TenLoai).HasColumnName("TenLoai").HasMaxLength(50).IsRequired();
        builder.Property(e => e.MoTa).HasColumnName("MoTa").HasMaxLength(255);
        builder.Property(e => e.IsActive).HasColumnName("IsActive");
    }
}

public class KhTinhTrangKhachHangConfiguration : IEntityTypeConfiguration<KhTinhTrangKhachHang>
{
    public void Configure(EntityTypeBuilder<KhTinhTrangKhachHang> builder)
    {
        builder.ToTable("KH_TinhTrangKhachHang");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id");
        builder.Property(e => e.TenTinhTrang).HasColumnName("TenTinhTrang").HasMaxLength(100).IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("IsActive");
    }
}

public class SysAuditLogConfiguration : IEntityTypeConfiguration<SysAuditLog>
{
    public void Configure(EntityTypeBuilder<SysAuditLog> builder)
    {
        builder.ToTable("SYS_AuditLog");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(e => e.TableName).HasColumnName("TableName").HasMaxLength(50).IsRequired();
        builder.Property(e => e.RecordId).HasColumnName("RecordId");
        builder.Property(e => e.Action).HasColumnName("Action").HasMaxLength(10).IsRequired();
        builder.Property(e => e.OldData).HasColumnName("OldData").HasColumnType("json");
        builder.Property(e => e.NewData).HasColumnName("NewData").HasColumnType("json");
        builder.Property(e => e.UserId).HasColumnName("UserId");
        builder.Property(e => e.ChangedAt).HasColumnName("ChangedAt");
    }
}
