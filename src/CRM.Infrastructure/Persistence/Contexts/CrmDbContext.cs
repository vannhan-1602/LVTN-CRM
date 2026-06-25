using CRM.Infrastructure.Persistence.Configurations;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Contexts;

public partial class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }

    // Identity / HR (HT_)
    public virtual DbSet<HtUserEntity> HtUsers { get; set; }
    public virtual DbSet<HtRoleEntity> HtRoles { get; set; }
    public virtual DbSet<HtThongTinNhanSuEntity> HtThongTinNhanSu { get; set; }
    public virtual DbSet<HtPhongBanEntity> HtPhongBans { get; set; }
    public virtual DbSet<HtChucVuEntity> HtChucVus { get; set; }

    // Khách hàng (KH_)
    public virtual DbSet<KhKhachHangEntity> KhKhachHangs { get; set; }
    public virtual DbSet<KhLoaiKhachHangEntity> KhLoaiKhachHangs { get; set; }
    public virtual DbSet<KhTinhTrangKhachHangEntity> KhTinhTrangKhachHangs { get; set; }
    public virtual DbSet<KhLeadEntity> KhLeads { get; set; }
    public virtual DbSet<KhHoatDongEntity> KhHoatDongs { get; set; }
    public virtual DbSet<KhDiaChiEntity> KhDiaChis { get; set; }

    // Bán hàng (BH_)
    public virtual DbSet<BhLoaiSanPhamEntity> BhLoaiSanPhams { get; set; }
    public virtual DbSet<BhSanPhamEntity> BhSanPhams { get; set; }
    public virtual DbSet<BhSanPhamHinhAnhEntity> BhSanPhamHinhAnhs { get; set; }
    public virtual DbSet<BhCoHoiBanHangEntity> BhCoHoiBanHangs { get; set; }

    // Kho (Kho_)
    public virtual DbSet<KhoTheKhoEntity> KhoTheKhos { get; set; }

    // Báo giá + Hợp đồng (HD_)
    public virtual DbSet<HdBaoGiaEntity> HdBaoGias { get; set; }
    public virtual DbSet<HdBaoGiaChiTietEntity> HdBaoGiaChiTiets { get; set; }
    public virtual DbSet<HdHopDongEntity> HdHopDongs { get; set; }

    // Kế toán (KT_)
    public virtual DbSet<KtHoaDonEntity> KtHoaDons { get; set; }
    public virtual DbSet<KtPhieuThuChiEntity> KtPhieuThuChis { get; set; }

    // Ticket (TK_)
    public virtual DbSet<TkTicketEntity> TkTickets { get; set; }
    public virtual DbSet<TkLoaiTicketEntity> TkLoaiTickets { get; set; }
    public virtual DbSet<TkTicketPhanHoiEntity> TkTicketPhanHois { get; set; }

    // Hệ thống (SYS_)
    public virtual DbSet<SysAuditLogEntity> SysAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}