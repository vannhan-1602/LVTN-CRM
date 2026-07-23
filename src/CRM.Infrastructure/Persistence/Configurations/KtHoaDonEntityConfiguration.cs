using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class KtHoaDonEntityConfiguration : IEntityTypeConfiguration<KtHoaDonEntity>
{
    public void Configure(EntityTypeBuilder<KtHoaDonEntity> b)
    {
        b.ToTable("KT_HoaDon");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.MaHoaDon).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.MaHoaDon).IsUnique();
        b.Property(x => x.TongTien).HasColumnType("decimal(18,2)").IsRequired();
        b.Property(x => x.SoTienDaThu).HasColumnType("decimal(18,2)").HasDefaultValue(0m).IsRequired(false);
        b.Property(x => x.TrangThaiThanhToan).HasMaxLength(30).HasDefaultValue("ChuaThanhToan");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        b.Property(x => x.HopDongId).HasColumnName("HopDong_Id").IsRequired(false);
        b.Property(x => x.LichThanhToanId).HasColumnName("LichThanhToan_Id").IsRequired(false);

        b.HasOne<HdHopDongEntity>()
         .WithMany()
         .HasForeignKey(x => x.HopDongId)
         .OnDelete(DeleteBehavior.SetNull);

        // Đợt trả góp (HD_LichThanhToan) mà hóa đơn này ứng với — NULL nếu thanh toán 1 lần
        // hoặc bán lẻ, chỉ set khi hợp đồng trả góp và kế toán chọn đúng đợt.
        b.HasOne<HdLichThanhToanEntity>()
         .WithMany()
         .HasForeignKey(x => x.LichThanhToanId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasOne<KhKhachHangEntity>()
         .WithMany()
         .HasForeignKey("KhachHang_Id")
         .OnDelete(DeleteBehavior.Restrict);
    }
}