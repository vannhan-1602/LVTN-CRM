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

        b.HasOne<HdHopDongEntity>()
         .WithMany()
         .HasForeignKey(x => x.HopDong_Id)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasOne<KhKhachHangEntity>()
         .WithMany()
         .HasForeignKey(x => x.KhachHang_Id)
         .OnDelete(DeleteBehavior.Restrict);
    }
}