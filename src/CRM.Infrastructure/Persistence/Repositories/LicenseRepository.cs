using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class LicenseRepository : ILicenseRepository
{
    private readonly CrmDbContext _context;
    public LicenseRepository(CrmDbContext context) => _context = context;

    public async Task<List<LicenseDto>> GetByHopDongAsync(ulong hopDongId, CancellationToken ct = default) =>
        await _context.HdLicenses
            .AsNoTracking()
            .Include(x => x.SanPham)
            .Where(x => x.HopDong_Id == hopDongId)
            .OrderByDescending(x => x.Id)
            .Select(x => ToDto(x))
            .ToListAsync(ct);

    public async Task<LicenseDto?> GetByIdAsync(ulong id, CancellationToken ct = default)
    {
        var e = await _context.HdLicenses
            .AsNoTracking()
            .Include(x => x.SanPham)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : ToDto(e);
    }

    public async Task<LicenseDto> AddAsync(
        ulong hopDongId, uint sanPhamId, int soLuongUser, string? phienBan,
        string moiTruongTrienKhai, DateOnly ngayKichHoat, DateOnly? ngayHetHan,
        CancellationToken ct = default)
    {
        var entity = new HdLicenseEntity
        {
            HopDong_Id = hopDongId,
            SanPham_Id = sanPhamId,
            SoLuongUser = soLuongUser,
            PhienBan = phienBan,
            MaLicenseKey = GenerateLicenseKey(),
            MoiTruongTrienKhai = moiTruongTrienKhai,
            NgayKichHoat = ngayKichHoat,
            NgayHetHan = ngayHetHan,
            TrangThai = "DangHoatDong",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.HdLicenses.Add(entity);
        await _context.SaveChangesAsync(ct);

        // Nạp lại kèm SanPham để trả về TenSanPham cho FE hiển thị ngay sau khi tạo.
        await _context.Entry(entity).Reference(x => x.SanPham).LoadAsync(ct);
        return ToDto(entity);
    }

    public async Task<LicenseDto?> RenewAsync(ulong id, DateOnly ngayHetHanMoi, CancellationToken ct = default)
    {
        var entity = await _context.HdLicenses.Include(x => x.SanPham)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return null;

        entity.NgayHetHan = ngayHetHanMoi;
        // Chỉ tự chuyển sang DangHoatDong khi đang HetHan (đúng mục đích chính của Renew).
        // Nếu đang TamKhoa thì GIỮ NGUYÊN — Renew không được tự ý mở khóa License đang bị
        // Manager khóa có chủ đích (xem comment trong RenewLicenseCommand.cs).
        if (entity.TrangThai != "TamKhoa")
            entity.TrangThai = "DangHoatDong";
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<LicenseDto?> ToggleLockAsync(ulong id, bool khoa, CancellationToken ct = default)
    {
        var entity = await _context.HdLicenses.Include(x => x.SanPham)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return null;

        entity.TrangThai = khoa ? "TamKhoa" : "DangHoatDong";
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<int> MarkExpiredAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expired = await _context.HdLicenses
            .Where(x => x.TrangThai != "HetHan" && x.NgayHetHan.HasValue && x.NgayHetHan.Value < today)
            .ToListAsync(ct);

        if (expired.Count == 0) return 0;

        foreach (var e in expired)
        {
            e.TrangThai = "HetHan";
            e.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync(ct);
        return expired.Count;
    }

    public async Task<List<LicenseDto>> GetExpiringSoonAsync(int soNgayConLai, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var moc = today.AddDays(soNgayConLai);
        return await _context.HdLicenses
            .AsNoTracking()
            .Include(x => x.SanPham)
            .Include(x => x.HopDong)
            .Where(x => x.TrangThai == "DangHoatDong" && x.NgayHetHan.HasValue
                     && x.NgayHetHan.Value >= today && x.NgayHetHan.Value == moc)
            .Select(x => ToDto(x))
            .ToListAsync(ct);
    }

    private static string GenerateLicenseKey() =>
        "LIC-" + Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8)).ToUpperInvariant();

    private static LicenseDto ToDto(HdLicenseEntity e) => new()
    {
        Id = e.Id,
        HopDongId = e.HopDong_Id,
        SanPhamId = e.SanPham_Id,
        TenSanPham = e.SanPham?.TenSP,
        SoLuongUser = e.SoLuongUser,
        PhienBan = e.PhienBan,
        MaLicenseKey = e.MaLicenseKey,
        MoiTruongTrienKhai = e.MoiTruongTrienKhai,
        NgayKichHoat = e.NgayKichHoat,
        NgayHetHan = e.NgayHetHan,
        TrangThai = e.TrangThai,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}