using CRM.Infrastructure.Persistence.Configurations;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Contexts;

public partial class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }

    // Identity / HR
    public virtual DbSet<HtUser> HtUsers { get; set; }
    public virtual DbSet<HtRole> HtRoles { get; set; }
    public virtual DbSet<HtThongTinNhanSu> HtThongTinNhanSu { get; set; }
    public virtual DbSet<HtPhongBan> HtPhongBans { get; set; }
    public virtual DbSet<HtChucVu> HtChucVus { get; set; }

    // Khách hàng
    public virtual DbSet<KhKhachHang> KhKhachHangs { get; set; }
    public virtual DbSet<KhLoaiKhachHang> KhLoaiKhachHangs { get; set; }
    public virtual DbSet<KhTinhTrangKhachHang> KhTinhTrangKhachHangs { get; set; }
    public DbSet<KhLeadEntity> KhLeads { get; set; }

    // Ticket
    public DbSet<TkTicketEntity> TkTickets { get; set; }
    public DbSet<TkLoaiTicketEntity> TkLoaiTickets { get; set; }
    public DbSet<TkTicketPhanHoiEntity> TkTicketPhanHois { get; set; }

    //Sản phẩm/Dịch vụ + Kho
    public DbSet<BhLoaiSanPhamEntity> BhLoaiSanPhams { get; set; }
    public DbSet<BhSanPhamEntity> BhSanPhams { get; set; }
    public DbSet<BhSanPhamHinhAnhEntity> BhSanPhamHinhAnhs { get; set; }
    public DbSet<KhoTheKhoEntity> KhoTheKhos { get; set; }

    // Báo giá + Hợp đồng
    public DbSet<HdBaoGiaEntity> HdBaoGias { get; set; }
    public DbSet<HdBaoGiaChiTietEntity> HdBaoGiaChiTiets { get; set; }
    public DbSet<HdHopDongEntity> HdHopDongs { get; set; }

    // Audit
    public virtual DbSet<SysAuditLog> SysAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
