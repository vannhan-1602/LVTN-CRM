using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HtPhongBanEntityConfiguration : IEntityTypeConfiguration<HtPhongBanEntity>
{
    public void Configure(EntityTypeBuilder<HtPhongBanEntity> builder)
    {
        builder.ToTable("HT_PhongBan");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(e => e.TenPhongBan).HasColumnName("TenPhongBan").HasMaxLength(100).IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("IsActive");

        builder.HasIndex(e => e.TenPhongBan).IsUnique();
    }
}