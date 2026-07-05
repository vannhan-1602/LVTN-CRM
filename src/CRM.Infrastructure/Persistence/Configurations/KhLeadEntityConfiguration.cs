using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations
{
    public class KhLeadEntityConfiguration : IEntityTypeConfiguration<KhLeadEntity>
    {
        public void Configure(EntityTypeBuilder<KhLeadEntity> builder)
        {
            builder.ToTable("KH_Lead");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.TenLead).HasMaxLength(150).IsRequired();
            builder.Property(x => x.TenCongTy).HasMaxLength(255);
            builder.Property(x => x.SoDienThoai).HasMaxLength(20);
            builder.Property(x => x.Email).HasMaxLength(150);
            builder.Property(x => x.TinhTrang).HasMaxLength(50);
            builder.Property(x => x.NhanVienPhuTrach_Id).HasColumnName("NhanVienPhuTrach_Id");
            builder.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            builder.Property(x => x.CreatedAt).ValueGeneratedOnAdd();
            builder.Property(x => x.UpdatedAt).ValueGeneratedOnAddOrUpdate();
        }
    }
}
