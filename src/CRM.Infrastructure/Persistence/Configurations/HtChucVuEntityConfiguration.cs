using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HtChucVuEntityConfiguration : IEntityTypeConfiguration<HtChucVuEntity>
{
    public void Configure(EntityTypeBuilder<HtChucVuEntity> builder)
    {
        builder.ToTable("HT_ChucVu");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(e => e.TenChucVu).HasColumnName("TenChucVu").HasMaxLength(100).IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("IsActive");

        builder.HasIndex(e => e.TenChucVu).IsUnique();
    }
}