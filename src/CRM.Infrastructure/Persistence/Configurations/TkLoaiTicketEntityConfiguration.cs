using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class TkLoaiTicketEntityConfiguration
    : IEntityTypeConfiguration<TkLoaiTicketEntity>
{
    public void Configure(EntityTypeBuilder<TkLoaiTicketEntity> builder)
    {
        builder.ToTable("TK_LoaiTicket");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.TenLoai)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.MoTa)
            .HasMaxLength(255);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.TenLoai)
            .IsUnique();
    }
}