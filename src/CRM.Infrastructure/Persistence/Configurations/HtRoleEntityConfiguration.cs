using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HtRoleEntityConfiguration : IEntityTypeConfiguration<HtRoleEntity>
{
    public void Configure(EntityTypeBuilder<HtRoleEntity> builder)
    {
        builder.ToTable("HT_Role");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(e => e.TenRole).HasColumnName("TenRole").HasMaxLength(100).IsRequired();
        builder.Property(e => e.MoTa).HasColumnName("MoTa").HasMaxLength(255);
    }
}