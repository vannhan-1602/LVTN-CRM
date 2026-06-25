using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class KhTinhTrangKhachHangEntityConfiguration : IEntityTypeConfiguration<KhTinhTrangKhachHangEntity>
{
    public void Configure(EntityTypeBuilder<KhTinhTrangKhachHangEntity> builder)
    {
        builder.ToTable("KH_TinhTrangKhachHang");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id");
        builder.Property(e => e.TenTinhTrang).HasColumnName("TenTinhTrang").HasMaxLength(100).IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("IsActive");
    }
}