using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class TkSlaEntityConfiguration : IEntityTypeConfiguration<TkSlaEntity>
{
    public void Configure(EntityTypeBuilder<TkSlaEntity> b)
    {
        b.ToTable("TK_SLA");
        b.HasKey(x => x.MucDoUuTien);
        b.Property(x => x.MucDoUuTien).HasMaxLength(20).ValueGeneratedNever();
        b.Property(x => x.SoGioPhanHoi).IsRequired();
        b.Property(x => x.SoGioXuLy).IsRequired();
    }
}
