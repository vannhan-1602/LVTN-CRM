using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class TkDanhGiaHaiLongEntityConfiguration : IEntityTypeConfiguration<TkDanhGiaHaiLongEntity>
{
    public void Configure(EntityTypeBuilder<TkDanhGiaHaiLongEntity> b)
    {
        b.ToTable("TK_DanhGiaHaiLong");
        b.HasKey(x => x.Ticket_Id);
        b.Property(x => x.Ticket_Id).ValueGeneratedNever();
        b.Property(x => x.Token).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Token).IsUnique();
        b.Property(x => x.DaGuiEmail).HasDefaultValue(false);

        b.HasOne(x => x.Ticket)
         .WithOne()
         .HasForeignKey<TkDanhGiaHaiLongEntity>(x => x.Ticket_Id)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
