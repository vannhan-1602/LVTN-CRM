using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class BhCoHoiBanHangEntityConfiguration : IEntityTypeConfiguration<BhCoHoiBanHangEntity>
{
    public void Configure(EntityTypeBuilder<BhCoHoiBanHangEntity> b)
    {
        b.ToTable("BH_CoHoiBanHang");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.TenThuongVu).HasMaxLength(100).IsRequired();
        b.Property(x => x.GiaiDoan).HasMaxLength(50).HasDefaultValue("KhaoSat");
        b.Property(x => x.TyLeThanhCong).HasDefaultValue(0);
        b.Property(x => x.DoanhThuKyVong).HasColumnType("decimal(18,2)");
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}