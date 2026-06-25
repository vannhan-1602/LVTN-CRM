using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HdHopDongEntityConfiguration : IEntityTypeConfiguration<HdHopDongEntity>
{
    public void Configure(EntityTypeBuilder<HdHopDongEntity> b)
    {
        b.ToTable("HD_HopDong");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.MaHopDong).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.MaHopDong).IsUnique();
        b.Property(x => x.KhachHang_Id).IsRequired();
        b.Property(x => x.BaoGia_Id).IsRequired(false);
        b.Property(x => x.NgayKy).IsRequired(false);
        b.Property(x => x.ThoiHan).IsRequired(false);
        b.Property(x => x.TrangThai).HasMaxLength(30).HasDefaultValue("DangThucHien");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        b.HasOne<KhKhachHangEntity>()
         .WithMany()
         .HasForeignKey(x => x.KhachHang_Id)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne<HdBaoGiaEntity>()
         .WithMany()
         .HasForeignKey(x => x.BaoGia_Id)
         .IsRequired(false)
         .OnDelete(DeleteBehavior.SetNull);
    }
}