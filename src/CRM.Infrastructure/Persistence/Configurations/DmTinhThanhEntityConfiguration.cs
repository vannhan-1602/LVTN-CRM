using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class DmTinhThanhEntityConfiguration : IEntityTypeConfiguration<DmTinhThanhEntity>
{
    public void Configure(EntityTypeBuilder<DmTinhThanhEntity> b)
    {
        b.ToTable("DM_TinhThanh");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.TenTinhThanh).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.TenTinhThanh).IsUnique();
    }
}