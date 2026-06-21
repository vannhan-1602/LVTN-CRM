using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class TkTicketPhanHoiConfiguration
    : IEntityTypeConfiguration<TkTicketPhanHoiEntity>
{
    public void Configure(EntityTypeBuilder<TkTicketPhanHoiEntity> builder)
    {
        builder.ToTable("TK_Ticket_PhanHoi");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LoaiPhanHoi)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.NoiDung)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.FileDinhKem)
            .HasMaxLength(500);

        builder.Property(x => x.TrangThaiTruoc)
            .HasMaxLength(20);

        builder.Property(x => x.TrangThaiSau)
            .HasMaxLength(20);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp");

        builder.HasIndex(x => x.Ticket_Id);

        builder.HasIndex(x => x.NguoiPhanHoi_Id);
    }
}