using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class KtPhieuThuChiEntityConfiguration : IEntityTypeConfiguration<KtPhieuThuChiEntity>
{
    public void Configure(EntityTypeBuilder<KtPhieuThuChiEntity> b)
    {
        b.ToTable("KT_PhieuThuChi");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();

        b.Property(x => x.MaPhieu)
         .HasColumnName("MaPhieu")
         .HasMaxLength(50)
         .IsRequired();
        b.HasIndex(x => x.MaPhieu).IsUnique();

        b.Property(x => x.LoaiPhieu)
         .HasColumnName("LoaiPhieu")
         .HasMaxLength(10)
         .HasDefaultValue("Thu")
         .IsRequired();

        b.Property(x => x.KhachHang_Id).HasColumnName("KhachHang_Id");
        b.Property(x => x.HoaDon_Id).HasColumnName("HoaDon_Id");
        b.Property(x => x.SoTien)
         .HasColumnName("SoTien")
         .HasColumnType("decimal(18,2)")
         .IsRequired();

        b.Property(x => x.NguoiLap_Id).HasColumnName("NguoiLap_Id");

        b.Property(x => x.NgayTao)
         .HasColumnName("NgayTao")
         .HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.UpdatedAt)
         .HasColumnName("UpdatedAt")
         .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
         .ValueGeneratedOnAddOrUpdate();

        // FK về KT_HoaDon — cùng kiểu ulong, an toàn
        b.HasOne<KtHoaDonEntity>()
         .WithMany()
         .HasForeignKey(x => x.HoaDon_Id)
         .OnDelete(DeleteBehavior.Restrict);

        // FK về KH_KhachHang — cùng kiểu ulong, an toàn
        b.HasOne<KhKhachHangEntity>()
         .WithMany()
         .HasForeignKey(x => x.KhachHang_Id)
         .OnDelete(DeleteBehavior.Restrict);

        // FK về HT_User — NguoiLap_Id (uint?) → HtUserEntity.Id (uint)
        // Cùng kiểu sau khi sửa entity, EF Core xử lý nullable FK bình thường
        b.HasOne<HtUserEntity>()
         .WithMany()
         .HasForeignKey(x => x.NguoiLap_Id)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
