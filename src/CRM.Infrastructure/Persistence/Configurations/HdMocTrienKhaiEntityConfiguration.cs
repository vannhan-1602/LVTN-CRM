using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HdMocTrienKhaiEntityConfiguration : IEntityTypeConfiguration<HdMocTrienKhaiEntity>
{
    public void Configure(EntityTypeBuilder<HdMocTrienKhaiEntity> b)
    {
        b.ToTable("HD_MocTrienKhai");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.HopDong_Id).IsRequired();
        b.Property(x => x.LoaiMoc).HasMaxLength(20).IsRequired();
        b.Property(x => x.NoiDung).HasMaxLength(255);
        b.Property(x => x.NguoiXacNhanKhach).HasMaxLength(255);
        b.Property(x => x.FileBienBan).HasMaxLength(500);
        b.Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("ChuaThucHien");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        b.HasOne(x => x.HopDong)
         .WithMany()
         .HasForeignKey(x => x.HopDong_Id)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.NhanVienThucHien)
         .WithMany()
         .HasForeignKey(x => x.NhanVienThucHien_Id)
         .IsRequired(false)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
