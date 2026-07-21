using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HdLichThanhToanEntityConfiguration : IEntityTypeConfiguration<HdLichThanhToanEntity>
{
    public void Configure(EntityTypeBuilder<HdLichThanhToanEntity> b)
    {
        b.ToTable("HD_LichThanhToan");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.HopDong_Id).IsRequired();
        b.Property(x => x.SoDot).IsRequired();
        b.Property(x => x.SoTien).HasColumnType("decimal(18,2)").IsRequired();
        b.Property(x => x.HanThanhToan).IsRequired();
        b.Property(x => x.TrangThai).HasMaxLength(30).HasDefaultValue("ChuaDenHan");

        b.HasOne(x => x.HopDong)
         .WithMany(x => x.LichThanhToans)
         .HasForeignKey(x => x.HopDong_Id)
         .OnDelete(DeleteBehavior.Cascade);
    }
}