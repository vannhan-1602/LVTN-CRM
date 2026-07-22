using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class ContractMilestoneRepository : IContractMilestoneRepository
{
    private readonly CrmDbContext _context;
    public ContractMilestoneRepository(CrmDbContext context) => _context = context;

    public async Task<List<MocTrienKhaiDto>> GetByHopDongAsync(ulong hopDongId, CancellationToken ct = default) =>
        await _context.HdMocTrienKhais
            .AsNoTracking()
            .Where(x => x.HopDong_Id == hopDongId)
            .OrderBy(x => x.LoaiMoc).ThenBy(x => x.Id)
            .Select(x => ToDto(x, null))
            .ToListAsync(ct);

    public async Task<MocTrienKhaiDto?> GetByIdAsync(ulong id, CancellationToken ct = default)
    {
        var e = await _context.HdMocTrienKhais.Include(x => x.NhanVienThucHien!).ThenInclude(u => u.NhanSu)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : ToDto(e, e.NhanVienThucHien?.NhanSu?.HoTen);
    }

    public async Task<MocTrienKhaiDto> AddAsync(
        ulong hopDongId, string loaiMoc, string? noiDung,
        DateTime? ngayThucHien, uint? nhanVienThucHienId, CancellationToken ct = default)
    {
        var entity = new HdMocTrienKhaiEntity
        {
            HopDong_Id = hopDongId,
            LoaiMoc = loaiMoc,
            NoiDung = noiDung,
            NgayThucHien = ngayThucHien,
            NhanVienThucHien_Id = nhanVienThucHienId,
            TrangThai = "ChuaThucHien",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.HdMocTrienKhais.Add(entity);
        await _context.SaveChangesAsync(ct);
        return ToDto(entity, null);
    }

    public async Task<MocTrienKhaiDto?> UpdateAsync(
        ulong id, string? noiDung, DateTime? ngayThucHien, uint? nhanVienThucHienId,
        string? nguoiXacNhanKhach, string? fileBienBan, string trangThai, CancellationToken ct = default)
    {
        var entity = await _context.HdMocTrienKhais.FindAsync(new object[] { id }, ct);
        if (entity is null) return null;

        entity.NoiDung = noiDung;
        entity.NgayThucHien = ngayThucHien;
        entity.NhanVienThucHien_Id = nhanVienThucHienId;
        entity.NguoiXacNhanKhach = nguoiXacNhanKhach;
        entity.FileBienBan = fileBienBan;
        entity.TrangThai = trangThai;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return ToDto(entity, null);
    }

    public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
    {
        var entity = await _context.HdMocTrienKhais.FindAsync(new object[] { id }, ct);
        if (entity is null) return false;
        _context.HdMocTrienKhais.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private static MocTrienKhaiDto ToDto(HdMocTrienKhaiEntity e, string? tenNhanVien) => new()
    {
        Id = e.Id,
        HopDongId = e.HopDong_Id,
        LoaiMoc = e.LoaiMoc,
        NoiDung = e.NoiDung,
        NgayThucHien = e.NgayThucHien,
        NhanVienThucHienId = e.NhanVienThucHien_Id,
        TenNhanVienThucHien = tenNhanVien,
        NguoiXacNhanKhach = e.NguoiXacNhanKhach,
        FileBienBan = e.FileBienBan,
        TrangThai = e.TrangThai,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
