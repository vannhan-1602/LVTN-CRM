using CRM.Infrastructure.Persistence.Configurations;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Contexts;

public partial class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }

    public virtual DbSet<HtUser> HtUsers { get; set; }
    public virtual DbSet<HtRole> HtRoles { get; set; }
    public virtual DbSet<HtThongTinNhanSu> HtThongTinNhanSu { get; set; }
    public virtual DbSet<KhKhachHang> KhKhachHangs { get; set; }
    public virtual DbSet<KhLoaiKhachHang> KhLoaiKhachHangs { get; set; }
    public virtual DbSet<KhTinhTrangKhachHang> KhTinhTrangKhachHangs { get; set; }
    public DbSet<KhLeadEntity> KhLeads { get; set; }
    public virtual DbSet<SysAuditLog> SysAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
