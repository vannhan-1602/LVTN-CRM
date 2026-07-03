using CRM.Application.Features.DanhMuc.DTOs;

namespace CRM.Application.Interfaces.DanhMuc;

public interface IDanhMucRepository
{
    // ── LoaiKhachHang ─────────────────────────────────────────────────────────
    Task<List<LoaiKhachHangDto>> GetAllLoaiKhachHangAsync(CancellationToken ct = default);
    Task<LoaiKhachHangDto?> GetLoaiKhachHangByIdAsync(ushort id, CancellationToken ct = default);
    Task<LoaiKhachHangDto> CreateLoaiKhachHangAsync(UpsertLoaiKhachHangDto dto, CancellationToken ct = default);
    Task<LoaiKhachHangDto> UpdateLoaiKhachHangAsync(ushort id, UpsertLoaiKhachHangDto dto, CancellationToken ct = default);
    Task DeleteLoaiKhachHangAsync(ushort id, CancellationToken ct = default);

    // ── TinhTrangKhachHang ────────────────────────────────────────────────────
    Task<List<TinhTrangKhachHangDto>> GetAllTinhTrangAsync(CancellationToken ct = default);
    Task<TinhTrangKhachHangDto?> GetTinhTrangByIdAsync(ushort id, CancellationToken ct = default);
    Task<TinhTrangKhachHangDto> CreateTinhTrangAsync(UpsertTinhTrangKhachHangDto dto, CancellationToken ct = default);
    Task<TinhTrangKhachHangDto> UpdateTinhTrangAsync(ushort id, UpsertTinhTrangKhachHangDto dto, CancellationToken ct = default);
    Task DeleteTinhTrangAsync(ushort id, CancellationToken ct = default);

    // ── LoaiTicket ────────────────────────────────────────────────────────────
    Task<List<LoaiTicketDto>> GetAllLoaiTicketAsync(CancellationToken ct = default);
    Task<LoaiTicketDto?> GetLoaiTicketByIdAsync(ushort id, CancellationToken ct = default);
    Task<LoaiTicketDto> CreateLoaiTicketAsync(UpsertLoaiTicketDto dto, CancellationToken ct = default);
    Task<LoaiTicketDto> UpdateLoaiTicketAsync(ushort id, UpsertLoaiTicketDto dto, CancellationToken ct = default);
    Task DeleteLoaiTicketAsync(ushort id, CancellationToken ct = default);

    // ── LoaiSanPham ───────────────────────────────────────────────────────────
    Task<List<LoaiSanPhamDto>> GetAllLoaiSanPhamAsync(CancellationToken ct = default);
    Task<LoaiSanPhamDto?> GetLoaiSanPhamByIdAsync(uint id, CancellationToken ct = default);
    Task<LoaiSanPhamDto> CreateLoaiSanPhamAsync(UpsertLoaiSanPhamDto dto, CancellationToken ct = default);
    Task<LoaiSanPhamDto> UpdateLoaiSanPhamAsync(uint id, UpsertLoaiSanPhamDto dto, CancellationToken ct = default);
    Task DeleteLoaiSanPhamAsync(uint id, CancellationToken ct = default);

    // ── XepHang ───────────────────────────────────────────────────────────────
    Task<List<XepHangDto>> GetAllXepHangAsync(CancellationToken ct = default);
    Task<XepHangDto?> GetXepHangByIdAsync(ushort id, CancellationToken ct = default);
    Task<XepHangDto> UpdateXepHangAsync(ushort id, UpdateXepHangDto dto, CancellationToken ct = default);
    // XepHang không cho tạo/xóa tự do vì gắn với logic nghiệp vụ tích điểm

    // ── NgayLe ────────────────────────────────────────────────────────────────
    Task<List<NgayLeDto>> GetAllNgayLeAsync(CancellationToken ct = default);
    Task<NgayLeDto?> GetNgayLeByIdAsync(ushort id, CancellationToken ct = default);
    Task<NgayLeDto> CreateNgayLeAsync(UpsertNgayLeDto dto, CancellationToken ct = default);
    Task<NgayLeDto> UpdateNgayLeAsync(ushort id, UpsertNgayLeDto dto, CancellationToken ct = default);
    Task DeleteNgayLeAsync(ushort id, CancellationToken ct = default);

    // ── Dùng chung cho các module khác ───────────────────────────────────────
    Task<List<XepHangDto>> GetXepHangActiveAsync(CancellationToken ct = default);
    Task<List<NgayLeDto>> GetNgayLeActiveAsync(CancellationToken ct = default);
}
