using CRM.Infrastructure.Persistence.Configurations;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Contexts;

public partial class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }

    public virtual DbSet<HtUserEntity> HtUsers { get; set; }
    public virtual DbSet<HtRoleEntity> HtRoles { get; set; }
    public virtual DbSet<HtThongTinNhanSuEntity> HtThongTinNhanSu { get; set; }
    public virtual DbSet<HtPhongBanEntity> HtPhongBans { get; set; }
    public virtual DbSet<HtChucVuEntity> HtChucVus { get; set; }

    public virtual DbSet<KhKhachHangEntity> KhKhachHangs { get; set; }
    public virtual DbSet<KhLoaiKhachHangEntity> KhLoaiKhachHangs { get; set; }
    public virtual DbSet<KhTinhTrangKhachHangEntity> KhTinhTrangKhachHangs { get; set; }
    public virtual DbSet<KhLeadEntity> KhLeads { get; set; }
    public virtual DbSet<KhHoatDongEntity> KhHoatDongs { get; set; }
    public virtual DbSet<KhDiaChiEntity> KhDiaChis { get; set; }

    public virtual DbSet<BhLoaiSanPhamEntity> BhLoaiSanPhams { get; set; }
    public virtual DbSet<BhSanPhamEntity> BhSanPhams { get; set; }
    public virtual DbSet<BhSanPhamHinhAnhEntity> BhSanPhamHinhAnhs { get; set; }
    public virtual DbSet<BhCoHoiBanHangEntity> BhCoHoiBanHangs { get; set; }

    public virtual DbSet<KhoTheKhoEntity> KhoTheKhos { get; set; }

    public virtual DbSet<HdBaoGiaEntity> HdBaoGias { get; set; }
    public virtual DbSet<HdBaoGiaChiTietEntity> HdBaoGiaChiTiets { get; set; }
    public virtual DbSet<HdHopDongEntity> HdHopDongs { get; set; }
    public virtual DbSet<HdLichThanhToanEntity> HdLichThanhToans { get; set; }
    public virtual DbSet<HdLicenseEntity> HdLicenses { get; set; }
    public virtual DbSet<HdMocTrienKhaiEntity> HdMocTrienKhais { get; set; }

    public virtual DbSet<KtHoaDonEntity> KtHoaDons { get; set; }
    public virtual DbSet<KtPhieuThuChiEntity> KtPhieuThuChis { get; set; }

    public virtual DbSet<TkTicketEntity> TkTickets { get; set; }
    public virtual DbSet<TkLoaiTicketEntity> TkLoaiTickets { get; set; }
    public virtual DbSet<TkTicketPhanHoiEntity> TkTicketPhanHois { get; set; }
    public virtual DbSet<TkSlaEntity> TkSlas { get; set; }
    public virtual DbSet<TkDanhGiaHaiLongEntity> TkDanhGiaHaiLongs { get; set; }

    public virtual DbSet<KhXepHangEntity> KhXepHangs { get; set; }
    public virtual DbSet<KhNgayLeEntity> KhNgayLes { get; set; }
    public virtual DbSet<KhDiemThuongEntity> KhDiemThuongs { get; set; }
    public virtual DbSet<KhLichSuHangEntity> KhLichSuHangs { get; set; }
    public virtual DbSet<KhVoucherEntity> KhVouchers { get; set; }
    public virtual DbSet<KhVoucherTokenEntity> KhVoucherTokens { get; set; }
    public virtual DbSet<KhEmailLogEntity> KhEmailLogs { get; set; }

    public virtual DbSet<SysAuditLogEntity> SysAuditLogs { get; set; }

    public virtual DbSet<DmTinhThanhEntity> DmTinhThanhs { get; set; }
    public virtual DbSet<DmPhuongXaEntity> DmPhuongXas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}