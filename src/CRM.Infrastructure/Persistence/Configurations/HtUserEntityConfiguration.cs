using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HtUserEntityConfiguration : IEntityTypeConfiguration<HtUserEntity>
{
    public void Configure(EntityTypeBuilder<HtUserEntity> builder)
    {
        builder.ToTable("HT_User");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(e => e.NhanSuId).HasColumnName("NhanSu_Id");
        builder.Property(e => e.Username).HasColumnName("Username").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Password).HasColumnName("Password").HasMaxLength(255).IsRequired();
        builder.Property(e => e.RoleId).HasColumnName("Role_Id");
        builder.Property(e => e.TrangThai).HasColumnName("TrangThai").HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.TokenVersion).HasColumnName("TokenVersion");
        builder.Property(e => e.SoLanDangNhapSai).HasColumnName("SoLanDangNhapSai");
        builder.Property(e => e.KhoaDenThoiDiem).HasColumnName("KhoaDenThoiDiem");
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");

        builder.HasIndex(e => e.Username).IsUnique();

        builder.HasOne(e => e.NhanSu)
            .WithOne(e => e.User)
            .HasForeignKey<HtUserEntity>(e => e.NhanSuId)
            .HasConstraintName("fk_user_nhansu");

        builder.HasOne(e => e.Role)
            .WithMany(e => e.Users)
            .HasForeignKey(e => e.RoleId)
            .HasConstraintName("fk_user_role");
    }
}