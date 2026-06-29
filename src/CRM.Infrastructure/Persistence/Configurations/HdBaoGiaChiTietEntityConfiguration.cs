using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HdBaoGiaChiTietEntityConfiguration : IEntityTypeConfiguration<HdBaoGiaChiTietEntity>
{
    public void Configure(EntityTypeBuilder<HdBaoGiaChiTietEntity> b)
    {
        b.ToTable("HD_BaoGia_ChiTiet");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.BaoGia_Id).IsRequired();
        b.Property(x => x.SanPham_Id).IsRequired();
        b.Property(x => x.SoLuong).IsRequired().HasDefaultValue(0);
        b.Property(x => x.DonGia).HasColumnType("decimal(18,2)").IsRequired();

        b.HasOne<HdBaoGiaEntity>()
         .WithMany()
         .HasForeignKey("BaoGia_Id")
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<BhSanPhamEntity>()
         .WithMany()
         .HasForeignKey("SanPham_Id")
         .OnDelete(DeleteBehavior.Restrict);
    }
}