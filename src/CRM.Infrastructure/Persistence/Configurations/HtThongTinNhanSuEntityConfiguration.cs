using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HtThongTinNhanSuEntityConfiguration : IEntityTypeConfiguration<HtThongTinNhanSuEntity>
{
    public void Configure(EntityTypeBuilder<HtThongTinNhanSuEntity> builder)
    {
        builder.ToTable("HT_ThongTinNhanSu");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(e => e.HoTen).HasColumnName("HoTen").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasColumnName("Email").HasMaxLength(150);
        builder.Property(e => e.SoDienThoai).HasColumnName("SoDienThoai").HasMaxLength(20);
        builder.Property(e => e.PhongBanId).HasColumnName("PhongBan_Id");
        builder.Property(e => e.ChucVuId).HasColumnName("ChucVu_Id");
        builder.Property(e => e.TrangThai).HasColumnName("TrangThai");
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");

        builder.HasIndex(e => e.Email).IsUnique();

        builder.HasOne(e => e.PhongBan)
            .WithMany(p => p.NhanSus)
            .HasForeignKey(e => e.PhongBanId)
            .HasConstraintName("fk_ns_phongban");

        builder.HasOne(e => e.ChucVu)
            .WithMany(c => c.NhanSus)
            .HasForeignKey(e => e.ChucVuId)
            .HasConstraintName("fk_ns_chucvu");
    }
}