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
        b.Property(x => x.KhachHangId).HasColumnName("KhachHang_Id").IsRequired();
        b.Property(x => x.BaoGiaId).HasColumnName("BaoGia_Id").IsRequired(false);
        b.Property(x => x.NgayKy).IsRequired(false);
        b.Property(x => x.ThoiHan).IsRequired(false);

        b.Property(x => x.NgayKetThuc).IsRequired(false);
        b.Property(x => x.HinhThucThanhToan).HasMaxLength(30).HasDefaultValue("ThanhToanMotLan");

        b.Property(x => x.TrangThai).HasMaxLength(30).HasDefaultValue("DangThucHien");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        b.HasOne<KhKhachHangEntity>()
         .WithMany()
         .HasForeignKey(x => x.KhachHangId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne<HdBaoGiaEntity>()
         .WithMany()
         .HasForeignKey(x => x.BaoGiaId)
         .IsRequired(false)
         .OnDelete(DeleteBehavior.SetNull);

        b.Property(x => x.LoaiHopDong).HasMaxLength(20).HasDefaultValue("ChinhThuc");
        b.Property(x => x.HopDongGocId).HasColumnName("HopDongGoc_Id").IsRequired(false);
        b.Property(x => x.NgayNhacGiaHanCuoi).IsRequired(false);

        b.HasOne<HdHopDongEntity>()
         .WithMany()
         .HasForeignKey(x => x.HopDongGocId)
         .IsRequired(false)
         .OnDelete(DeleteBehavior.Restrict);
    }
}