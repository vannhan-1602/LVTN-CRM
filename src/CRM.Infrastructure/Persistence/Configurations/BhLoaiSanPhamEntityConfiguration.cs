using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class BhLoaiSanPhamEntityConfiguration : IEntityTypeConfiguration<BhLoaiSanPhamEntity>
{
    public void Configure(EntityTypeBuilder<BhLoaiSanPhamEntity> b)
    {
        b.ToTable("BH_LoaiSanPham");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.TenLoai).HasMaxLength(150).IsRequired();
        b.Property(x => x.HinhThuc).HasMaxLength(20).HasDefaultValue("VatLy");
        b.Property(x => x.MoTa).HasColumnType("text");
    }
}
