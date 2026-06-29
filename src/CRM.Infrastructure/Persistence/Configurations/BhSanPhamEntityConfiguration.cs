using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class BhSanPhamEntityConfiguration : IEntityTypeConfiguration<BhSanPhamEntity>
{
    public void Configure(EntityTypeBuilder<BhSanPhamEntity> b)
    {
        b.ToTable("BH_SanPham");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.MaSP).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.MaSP).IsUnique();
        b.Property(x => x.TenSP).HasMaxLength(255).IsRequired();
        b.Property(x => x.DonVi).HasMaxLength(50).IsRequired(false);
        b.Property(x => x.GiaBan).HasColumnType("decimal(18,2)").HasDefaultValue(0m).IsRequired(false);
        b.Property(x => x.SoLuongTon).HasDefaultValue(0);
        b.Property(x => x.TrangThai).HasDefaultValue((byte)1);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        b.HasOne<BhLoaiSanPhamEntity>()
         .WithMany()
         .HasForeignKey("LoaiSanPham_Id")
         .IsRequired(false)
         .OnDelete(DeleteBehavior.Restrict);
    }
}