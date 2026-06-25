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
        b.Property(x => x.TinhThanh).HasMaxLength(50);
        b.Property(x => x.QuanHuyen).HasMaxLength(50);
        b.Property(x => x.PhuongXa).HasMaxLength(50);
        b.Property(x => x.IsDefault).HasDefaultValue(false);

        b.HasOne<KhKhachHangEntity>()
         .WithMany()
         .HasForeignKey(x => x.KhachHang_Id)
         .OnDelete(DeleteBehavior.Cascade);
    }
}