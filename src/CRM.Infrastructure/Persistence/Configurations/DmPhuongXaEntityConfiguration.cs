using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class DmPhuongXaEntityConfiguration : IEntityTypeConfiguration<DmPhuongXaEntity>
{
    public void Configure(EntityTypeBuilder<DmPhuongXaEntity> b)
    {
        b.ToTable("DM_PhuongXa");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.TinhThanh_Id).IsRequired();
        b.Property(x => x.TenPhuongXa).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.TinhThanh)
         .WithMany(x => x.PhuongXas)
         .HasForeignKey(x => x.TinhThanh_Id)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.TinhThanh_Id, x.TenPhuongXa }).IsUnique();
    }
}