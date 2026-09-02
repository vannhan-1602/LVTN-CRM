using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class HtRefreshTokenEntityConfiguration : IEntityTypeConfiguration<HtRefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<HtRefreshTokenEntity> builder)
    {
        builder.ToTable("HT_RefreshToken");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(e => e.UserId).HasColumnName("UserId").IsRequired();
        builder.Property(e => e.TokenHash).HasColumnName("TokenHash").HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnName("ExpiresAt").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").IsRequired();
        builder.Property(e => e.CreatedByIp).HasColumnName("CreatedByIp").HasMaxLength(45);
        builder.Property(e => e.RevokedAt).HasColumnName("RevokedAt");
        builder.Property(e => e.RevokedByIp).HasColumnName("RevokedByIp").HasMaxLength(45);
        builder.Property(e => e.ReplacedByTokenHash).HasColumnName("ReplacedByTokenHash").HasMaxLength(64).IsFixedLength();

        // Không map thuộc tính tính toán IsActive vào cột DB.
        builder.Ignore(e => e.IsActive);

        builder.HasIndex(e => e.TokenHash).IsUnique();
        builder.HasIndex(e => new { e.UserId, e.RevokedAt });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("fk_refreshtoken_user")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
