using CRM.Application.Common.Exceptions;
using CRM.Application.Features.DanhMuc.DTOs;
using CRM.Application.Interfaces.DanhMuc;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class DanhMucRepository : IDanhMucRepository
{
    private readonly CrmDbContext _context;
    public DanhMucRepository(CrmDbContext context) => _context = context;

    // ══════════════════════════════════════════════════════════════════════════
    // LOAI KHACH HANG
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<List<LoaiKhachHangDto>> GetAllLoaiKhachHangAsync(CancellationToken ct = default) =>
        await _context.KhLoaiKhachHangs
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => ToDto(x))
            .ToListAsync(ct);

    public async Task<LoaiKhachHangDto?> GetLoaiKhachHangByIdAsync(ushort id, CancellationToken ct = default) =>
        await _context.KhLoaiKhachHangs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ToDto(x))
            .FirstOrDefaultAsync(ct);

    public async Task<LoaiKhachHangDto> CreateLoaiKhachHangAsync(UpsertLoaiKhachHangDto dto, CancellationToken ct = default)
    {
        // Không cho trùng tên
        if (await _context.KhLoaiKhachHangs.AnyAsync(x => x.TenLoai == dto.TenLoai, ct))
            throw new BusinessRuleException($"Loại khách hàng '{dto.TenLoai}' đã tồn tại.");

        var entity = new KhLoaiKhachHangEntity
        {
            TenLoai = dto.TenLoai.Trim(),
            MoTa = dto.MoTa?.Trim(),
            IsActive = dto.IsActive
        };
        _context.KhLoaiKhachHangs.Add(entity);
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<LoaiKhachHangDto> UpdateLoaiKhachHangAsync(ushort id, UpsertLoaiKhachHangDto dto, CancellationToken ct = default)
    {
        var entity = await _context.KhLoaiKhachHangs.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Loại khách hàng", id);

        if (await _context.KhLoaiKhachHangs.AnyAsync(x => x.TenLoai == dto.TenLoai && x.Id != id, ct))
            throw new BusinessRuleException($"Loại khách hàng '{dto.TenLoai}' đã tồn tại.");

        entity.TenLoai = dto.TenLoai.Trim();
        entity.MoTa = dto.MoTa?.Trim();
        entity.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task DeleteLoaiKhachHangAsync(ushort id, CancellationToken ct = default)
    {
        var entity = await _context.KhLoaiKhachHangs.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Loại khách hàng", id);

        // Không cho xóa nếu đang có khách hàng dùng
        if (await _context.KhKhachHangs.AnyAsync(x => x.LoaiKhachHangId == id, ct))
            throw new BusinessRuleException("Không thể xóa vì đang có khách hàng thuộc loại này. Hãy vô hiệu hóa thay vì xóa.");

        _context.KhLoaiKhachHangs.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    private static LoaiKhachHangDto ToDto(KhLoaiKhachHangEntity e) => new()
    {
        Id = e.Id, TenLoai = e.TenLoai, MoTa = e.MoTa, IsActive = e.IsActive
    };

    // ══════════════════════════════════════════════════════════════════════════
    // TINH TRANG KHACH HANG
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<List<TinhTrangKhachHangDto>> GetAllTinhTrangAsync(CancellationToken ct = default) =>
        await _context.KhTinhTrangKhachHangs
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => ToDto(x))
            .ToListAsync(ct);

    public async Task<TinhTrangKhachHangDto?> GetTinhTrangByIdAsync(ushort id, CancellationToken ct = default) =>
        await _context.KhTinhTrangKhachHangs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ToDto(x))
            .FirstOrDefaultAsync(ct);

    public async Task<TinhTrangKhachHangDto> CreateTinhTrangAsync(UpsertTinhTrangKhachHangDto dto, CancellationToken ct = default)
    {
        if (await _context.KhTinhTrangKhachHangs.AnyAsync(x => x.TenTinhTrang == dto.TenTinhTrang, ct))
            throw new BusinessRuleException($"Tình trạng '{dto.TenTinhTrang}' đã tồn tại.");

        var entity = new KhTinhTrangKhachHangEntity
        {
            TenTinhTrang = dto.TenTinhTrang.Trim(),
            IsActive = dto.IsActive
        };
        _context.KhTinhTrangKhachHangs.Add(entity);
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<TinhTrangKhachHangDto> UpdateTinhTrangAsync(ushort id, UpsertTinhTrangKhachHangDto dto, CancellationToken ct = default)
    {
        var entity = await _context.KhTinhTrangKhachHangs.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Tình trạng khách hàng", id);

        if (await _context.KhTinhTrangKhachHangs.AnyAsync(x => x.TenTinhTrang == dto.TenTinhTrang && x.Id != id, ct))
            throw new BusinessRuleException($"Tình trạng '{dto.TenTinhTrang}' đã tồn tại.");

        entity.TenTinhTrang = dto.TenTinhTrang.Trim();
        entity.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task DeleteTinhTrangAsync(ushort id, CancellationToken ct = default)
    {
        var entity = await _context.KhTinhTrangKhachHangs.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Tình trạng khách hàng", id);

        if (await _context.KhKhachHangs.AnyAsync(x => x.TinhTrangId == id, ct))
            throw new BusinessRuleException("Không thể xóa vì đang có khách hàng ở tình trạng này.");

        _context.KhTinhTrangKhachHangs.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    private static TinhTrangKhachHangDto ToDto(KhTinhTrangKhachHangEntity e) => new()
    {
        Id = e.Id, TenTinhTrang = e.TenTinhTrang, IsActive = e.IsActive
    };

    // ══════════════════════════════════════════════════════════════════════════
    // LOAI TICKET
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<List<LoaiTicketDto>> GetAllLoaiTicketAsync(CancellationToken ct = default) =>
        await _context.TkLoaiTickets
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => ToDto(x))
            .ToListAsync(ct);

    public async Task<LoaiTicketDto?> GetLoaiTicketByIdAsync(ushort id, CancellationToken ct = default) =>
        await _context.TkLoaiTickets
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ToDto(x))
            .FirstOrDefaultAsync(ct);

    public async Task<LoaiTicketDto> CreateLoaiTicketAsync(UpsertLoaiTicketDto dto, CancellationToken ct = default)
    {
        if (await _context.TkLoaiTickets.AnyAsync(x => x.TenLoai == dto.TenLoai, ct))
            throw new BusinessRuleException($"Loại ticket '{dto.TenLoai}' đã tồn tại.");

        var entity = new TkLoaiTicketEntity
        {
            TenLoai = dto.TenLoai.Trim(),
            MoTa = dto.MoTa?.Trim(),
            IsActive = dto.IsActive
        };
        _context.TkLoaiTickets.Add(entity);
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<LoaiTicketDto> UpdateLoaiTicketAsync(ushort id, UpsertLoaiTicketDto dto, CancellationToken ct = default)
    {
        var entity = await _context.TkLoaiTickets.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Loại ticket", id);

        if (await _context.TkLoaiTickets.AnyAsync(x => x.TenLoai == dto.TenLoai && x.Id != id, ct))
            throw new BusinessRuleException($"Loại ticket '{dto.TenLoai}' đã tồn tại.");

        entity.TenLoai = dto.TenLoai.Trim();
        entity.MoTa = dto.MoTa?.Trim();
        entity.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task DeleteLoaiTicketAsync(ushort id, CancellationToken ct = default)
    {
        var entity = await _context.TkLoaiTickets.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Loại ticket", id);

        // Không xóa nếu đang có ticket dùng loại này
        if (await _context.TkTickets.AnyAsync(x => x.LoaiTicket_Id == id, ct))
            throw new BusinessRuleException("Không thể xóa vì đang có ticket thuộc loại này. Hãy vô hiệu hóa thay vì xóa.");

        _context.TkLoaiTickets.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    private static LoaiTicketDto ToDto(TkLoaiTicketEntity e) => new()
    {
        Id = e.Id, TenLoai = e.TenLoai, MoTa = e.MoTa, IsActive = e.IsActive
    };

    // ══════════════════════════════════════════════════════════════════════════
    // LOAI SAN PHAM
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<List<LoaiSanPhamDto>> GetAllLoaiSanPhamAsync(CancellationToken ct = default) =>
        await _context.BhLoaiSanPhams
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => ToDto(x))
            .ToListAsync(ct);

    public async Task<LoaiSanPhamDto?> GetLoaiSanPhamByIdAsync(uint id, CancellationToken ct = default) =>
        await _context.BhLoaiSanPhams
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ToDto(x))
            .FirstOrDefaultAsync(ct);

    public async Task<LoaiSanPhamDto> CreateLoaiSanPhamAsync(UpsertLoaiSanPhamDto dto, CancellationToken ct = default)
    {
        if (await _context.BhLoaiSanPhams.AnyAsync(x => x.TenLoai == dto.TenLoai, ct))
            throw new BusinessRuleException($"Loại sản phẩm '{dto.TenLoai}' đã tồn tại.");

        var entity = new BhLoaiSanPhamEntity
        {
            TenLoai = dto.TenLoai.Trim(),
            MoTa = dto.MoTa?.Trim()
        };
        _context.BhLoaiSanPhams.Add(entity);
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<LoaiSanPhamDto> UpdateLoaiSanPhamAsync(uint id, UpsertLoaiSanPhamDto dto, CancellationToken ct = default)
    {
        var entity = await _context.BhLoaiSanPhams.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Loại sản phẩm", id);

        if (await _context.BhLoaiSanPhams.AnyAsync(x => x.TenLoai == dto.TenLoai && x.Id != id, ct))
            throw new BusinessRuleException($"Loại sản phẩm '{dto.TenLoai}' đã tồn tại.");

        entity.TenLoai = dto.TenLoai.Trim();
        entity.MoTa = dto.MoTa?.Trim();
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task DeleteLoaiSanPhamAsync(uint id, CancellationToken ct = default)
    {
        var entity = await _context.BhLoaiSanPhams.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Loại sản phẩm", id);

        if (await _context.BhSanPhams.AnyAsync(x => x.LoaiSanPham_Id == id, ct))
            throw new BusinessRuleException("Không thể xóa vì đang có sản phẩm thuộc loại này.");

        _context.BhLoaiSanPhams.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    private static LoaiSanPhamDto ToDto(BhLoaiSanPhamEntity e) => new()
    {
        Id = e.Id, TenLoai = e.TenLoai, MoTa = e.MoTa
    };

    // ══════════════════════════════════════════════════════════════════════════
    // XEP HANG
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<List<XepHangDto>> GetAllXepHangAsync(CancellationToken ct = default) =>
        await _context.KhXepHangs
            .AsNoTracking()
            .OrderBy(x => x.ThuTu)
            .Select(x => ToDto(x))
            .ToListAsync(ct);

    public async Task<List<XepHangDto>> GetXepHangActiveAsync(CancellationToken ct = default) =>
        await _context.KhXepHangs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.ThuTu)
            .Select(x => ToDto(x))
            .ToListAsync(ct);

    public async Task<XepHangDto?> GetXepHangByIdAsync(ushort id, CancellationToken ct = default) =>
        await _context.KhXepHangs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ToDto(x))
            .FirstOrDefaultAsync(ct);

    public async Task<XepHangDto> UpdateXepHangAsync(ushort id, UpdateXepHangDto dto, CancellationToken ct = default)
    {
        var entity = await _context.KhXepHangs.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Hạng khách hàng", id);

        // Validate: mốc điểm phải tăng dần theo thứ tự hạng
        var hangTruoc = await _context.KhXepHangs
            .Where(x => x.ThuTu == entity.ThuTu - 1 && x.IsActive)
            .FirstOrDefaultAsync(ct);
        var hangSau = await _context.KhXepHangs
            .Where(x => x.ThuTu == entity.ThuTu + 1 && x.IsActive)
            .FirstOrDefaultAsync(ct);

        if (hangTruoc != null && dto.DiemToiThieu <= hangTruoc.DiemToiThieu)
            throw new BusinessRuleException(
                $"Mốc điểm của hạng '{entity.TenHang}' phải lớn hơn hạng '{hangTruoc.TenHang}' ({hangTruoc.DiemToiThieu} điểm).");

        if (hangSau != null && dto.DiemToiThieu >= hangSau.DiemToiThieu)
            throw new BusinessRuleException(
                $"Mốc điểm của hạng '{entity.TenHang}' phải nhỏ hơn hạng '{hangSau.TenHang}' ({hangSau.DiemToiThieu} điểm).");

        if (dto.PhanTramGiamVoucher < 0 || dto.PhanTramGiamVoucher > 100)
            throw new BusinessRuleException("Phần trăm giảm voucher phải từ 0 đến 100.");

        entity.DiemToiThieu = dto.DiemToiThieu;
        entity.SoLanThuToiThieu = dto.SoLanThuToiThieu;
        entity.PhanTramGiamVoucher = dto.PhanTramGiamVoucher;
        entity.MoTaQuyenLoi = dto.MoTaQuyenLoi?.Trim();
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    private static XepHangDto ToDto(KhXepHangEntity e) => new()
    {
        Id = e.Id,
        MaHang = e.MaHang,
        TenHang = e.TenHang,
        ThuTu = e.ThuTu,
        DiemToiThieu = e.DiemToiThieu,
        SoLanThuToiThieu = e.SoLanThuToiThieu,
        PhanTramGiamVoucher = e.PhanTramGiamVoucher,
        MoTaQuyenLoi = e.MoTaQuyenLoi,
        IsActive = e.IsActive
    };

    // ══════════════════════════════════════════════════════════════════════════
    // NGAY LE
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<List<NgayLeDto>> GetAllNgayLeAsync(CancellationToken ct = default) =>
        await _context.KhNgayLes
            .AsNoTracking()
            .OrderBy(x => x.Thang).ThenBy(x => x.Ngay)
            .Select(x => ToDto(x))
            .ToListAsync(ct);

    public async Task<List<NgayLeDto>> GetNgayLeActiveAsync(CancellationToken ct = default) =>
        await _context.KhNgayLes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Thang).ThenBy(x => x.Ngay)
            .Select(x => ToDto(x))
            .ToListAsync(ct);

    public async Task<NgayLeDto?> GetNgayLeByIdAsync(ushort id, CancellationToken ct = default) =>
        await _context.KhNgayLes
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ToDto(x))
            .FirstOrDefaultAsync(ct);

    public async Task<NgayLeDto> CreateNgayLeAsync(UpsertNgayLeDto dto, CancellationToken ct = default)
    {
        ValidateNgayLe(dto.Thang, dto.Ngay);

        if (await _context.KhNgayLes.AnyAsync(x => x.TenNgayLe == dto.TenNgayLe, ct))
            throw new BusinessRuleException($"Ngày lễ '{dto.TenNgayLe}' đã tồn tại.");

        var validLoaiKH = new[] { "B2C", "B2B", "TatCa" };
        if (!validLoaiKH.Contains(dto.ApDungChoLoaiKH))
            throw new BusinessRuleException("ApDungChoLoaiKH chỉ nhận: B2C, B2B, TatCa.");

        var entity = new KhNgayLeEntity
        {
            TenNgayLe = dto.TenNgayLe.Trim(),
            Thang = dto.Thang,
            Ngay = dto.Ngay,
            SoNgayGuiTruoc = dto.SoNgayGuiTruoc,
            ApDungChoLoaiKH = dto.ApDungChoLoaiKH,
            HangToiThieuApDung = dto.HangToiThieuApDung,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.KhNgayLes.Add(entity);
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<NgayLeDto> UpdateNgayLeAsync(ushort id, UpsertNgayLeDto dto, CancellationToken ct = default)
    {
        var entity = await _context.KhNgayLes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Ngày lễ", id);

        ValidateNgayLe(dto.Thang, dto.Ngay);

        if (await _context.KhNgayLes.AnyAsync(x => x.TenNgayLe == dto.TenNgayLe && x.Id != id, ct))
            throw new BusinessRuleException($"Ngày lễ '{dto.TenNgayLe}' đã tồn tại.");

        entity.TenNgayLe = dto.TenNgayLe.Trim();
        entity.Thang = dto.Thang;
        entity.Ngay = dto.Ngay;
        entity.SoNgayGuiTruoc = dto.SoNgayGuiTruoc;
        entity.ApDungChoLoaiKH = dto.ApDungChoLoaiKH;
        entity.HangToiThieuApDung = dto.HangToiThieuApDung;
        entity.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task DeleteNgayLeAsync(ushort id, CancellationToken ct = default)
    {
        var entity = await _context.KhNgayLes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Ngày lễ", id);
        _context.KhNgayLes.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    private static void ValidateNgayLe(byte thang, byte ngay)
    {
        if (thang < 1 || thang > 12)
            throw new BusinessRuleException("Tháng phải từ 1 đến 12.");
        if (ngay < 1 || ngay > 31)
            throw new BusinessRuleException("Ngày phải từ 1 đến 31.");
        // Kiểm tra ngày hợp lệ theo tháng (dùng năm không nhuận làm chuẩn)
        try { _ = new DateTime(2001, thang, ngay); }
        catch { throw new BusinessRuleException($"Ngày {ngay}/{thang} không hợp lệ."); }
    }

    private static NgayLeDto ToDto(KhNgayLeEntity e) => new()
    {
        Id = e.Id,
        TenNgayLe = e.TenNgayLe,
        Thang = e.Thang,
        Ngay = e.Ngay,
        SoNgayGuiTruoc = e.SoNgayGuiTruoc,
        ApDungChoLoaiKH = e.ApDungChoLoaiKH,
        HangToiThieuApDung = e.HangToiThieuApDung,
        IsActive = e.IsActive
    };
}
