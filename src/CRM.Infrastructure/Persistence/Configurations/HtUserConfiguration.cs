using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HtRoleConfiguration : IEntityTypeConfiguration<HtRole>
{
    public void Configure(EntityTypeBuilder<HtRole> builder)
    {
        builder.ToTable("HT_Role");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(e => e.TenRole).HasColumnName("TenRole").HasMaxLength(100).IsRequired();
        builder.Property(e => e.MoTa).HasColumnName("MoTa").HasMaxLength(255);
    }
}

public class HtThongTinNhanSuConfiguration : IEntityTypeConfiguration<HtThongTinNhanSu>
{
    public void Configure(EntityTypeBuilder<HtThongTinNhanSu> builder)
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

public class HtUserConfiguration : IEntityTypeConfiguration<HtUser>
{
    public void Configure(EntityTypeBuilder<HtUser> builder)
    {
        builder.ToTable("HT_User");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(e => e.NhanSuId).HasColumnName("NhanSu_Id");
        builder.Property(e => e.Username).HasColumnName("Username").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Password).HasColumnName("Password").HasMaxLength(255).IsRequired();
        builder.Property(e => e.RoleId).HasColumnName("Role_Id");
        builder.Property(e => e.TrangThai).HasColumnName("TrangThai").HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");

        builder.HasIndex(e => e.Username).IsUnique();

        builder.HasOne(e => e.NhanSu)
            .WithOne(e => e.User)
            .HasForeignKey<HtUser>(e => e.NhanSuId)
            .HasConstraintName("fk_user_nhansu");

        builder.HasOne(e => e.Role)
            .WithMany(e => e.Users)
            .HasForeignKey(e => e.RoleId)
            .HasConstraintName("fk_user_role");
    }
}
