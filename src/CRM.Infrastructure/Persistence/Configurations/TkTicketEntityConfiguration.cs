using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class TkTicketEntityConfiguration : IEntityTypeConfiguration<TkTicketEntity>
{
    public void Configure(EntityTypeBuilder<TkTicketEntity> builder)
    {
        builder.ToTable("TK_Ticket");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.MaTicket)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.MaTicket)
            .IsUnique();

        builder.Property(x => x.TieuDe)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.MoTa)
            .HasColumnType("text");

        builder.Property(x => x.FileDinhKem)
            .HasMaxLength(500);

        builder.Property(x => x.MucDoUuTien)
            .HasMaxLength(20)
            .HasDefaultValue("TrungBinh");

        builder.Property(x => x.NguonTiepNhan)
            .HasMaxLength(20)
            .HasDefaultValue("Phone");

        builder.Property(x => x.TrangThai)
            .HasMaxLength(20)
            .HasDefaultValue("Moi");

        builder.Property(x => x.LyDoDong)
            .HasMaxLength(500);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp");

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp");

        builder.HasIndex(x => new
        {
            x.TrangThai,
            x.IsDeleted
        });

        builder.HasIndex(x => x.MucDoUuTien);

        builder.HasIndex(x => x.KhachHang_Id);

        builder.HasIndex(x => x.NhanVienXuLy_Id);

        builder.Property(x => x.HopDongId).HasColumnName("HopDong_Id").IsRequired(false);

        // FK relationships
        builder.HasOne<KhKhachHangEntity>()
               .WithMany()
               .HasForeignKey("KhachHang_Id")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<HdHopDongEntity>()
               .WithMany()
               .HasForeignKey(x => x.HopDongId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}