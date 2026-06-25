using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class KhHoatDongEntityConfiguration : IEntityTypeConfiguration<KhHoatDongEntity>
{
    public void Configure(EntityTypeBuilder<KhHoatDongEntity> b)
    {
        b.ToTable("KH_HoatDong");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.LoaiHoatDong).HasMaxLength(20);
        b.Property(x => x.NoiDung).HasMaxLength(255);
        b.Property(x => x.ThoiGianThucHien);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        b.HasOne<KhKhachHangEntity>()
         .WithMany()
         .HasForeignKey(x => x.KhachHang_Id)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<KhLeadEntity>()
         .WithMany()
         .HasForeignKey(x => x.Lead_Id)
         .OnDelete(DeleteBehavior.Cascade);
    }
}