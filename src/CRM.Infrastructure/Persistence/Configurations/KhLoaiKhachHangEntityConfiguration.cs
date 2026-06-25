using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class KhLoaiKhachHangEntityConfiguration : IEntityTypeConfiguration<KhLoaiKhachHangEntity>
{
    public void Configure(EntityTypeBuilder<KhLoaiKhachHangEntity> builder)
    {
        builder.ToTable("KH_LoaiKhachHang");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id");
        builder.Property(e => e.TenLoai).HasColumnName("TenLoai").HasMaxLength(50).IsRequired();
        builder.Property(e => e.MoTa).HasColumnName("MoTa").HasMaxLength(255);
        builder.Property(e => e.IsActive).HasColumnName("IsActive");
    }
}