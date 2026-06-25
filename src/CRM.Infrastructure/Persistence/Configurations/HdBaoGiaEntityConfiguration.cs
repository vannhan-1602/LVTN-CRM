using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HdBaoGiaEntityConfiguration : IEntityTypeConfiguration<HdBaoGiaEntity>
{
    public void Configure(EntityTypeBuilder<HdBaoGiaEntity> b)
    {
        b.ToTable("HD_BaoGia");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.MaBaoGia).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.MaBaoGia).IsUnique();
        b.Property(x => x.KhachHang_Id).IsRequired();
        b.Property(x => x.TongTien).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        b.Property(x => x.TrangThai).HasMaxLength(30).HasDefaultValue("Nhap");
        b.Property(x => x.NhanVien_Id).IsRequired(false);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

        b.HasOne<KhKhachHangEntity>()
         .WithMany()
         .HasForeignKey(x => x.KhachHang_Id)
         .OnDelete(DeleteBehavior.Restrict);
    }
}