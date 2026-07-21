using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class KhDiaChiEntityConfiguration : IEntityTypeConfiguration<KhDiaChiEntity>
{
    public void Configure(EntityTypeBuilder<KhDiaChiEntity> b)
    {
        b.ToTable("KH_DiaChi");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.LoaiDiaChi).HasMaxLength(20);
        b.Property(x => x.DiaChiChiTiet).HasMaxLength(150);
        b.Property(x => x.TinhThanh_Id).IsRequired(false);
        b.Property(x => x.PhuongXa_Id).IsRequired(false);
        b.Property(x => x.IsDefault).HasDefaultValue(false);

        b.HasOne<KhKhachHangEntity>()
         .WithMany()
         .HasForeignKey("KhachHang_Id")
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.TinhThanh)
         .WithMany()
         .HasForeignKey(x => x.TinhThanh_Id)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.PhuongXa)
         .WithMany()
         .HasForeignKey(x => x.PhuongXa_Id)
         .OnDelete(DeleteBehavior.SetNull);
    }
}