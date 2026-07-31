using CRM.Application.Features.Contracts.DTOs;

namespace CRM.Application.Interfaces.Contracts;

/// <summary>Quản lý License phần mềm (HD_License) cấp cho khách hàng theo hợp đồng.
/// Không cho xóa cứng (dữ liệu có MaLicenseKey là lịch sử) — chỉ khóa (TamKhoa) hoặc để
/// job nền tự chuyển HetHan khi quá NgayHetHan.</summary>
public interface ILicenseRepository
{
    Task<List<LicenseDto>> GetByHopDongAsync(ulong hopDongId, CancellationToken ct = default);
    Task<LicenseDto?> GetByIdAsync(ulong id, CancellationToken ct = default);

    Task<LicenseDto> AddAsync(
        ulong hopDongId, uint sanPhamId, int soLuongUser, string? phienBan,
        string moiTruongTrienKhai, DateOnly ngayKichHoat, DateOnly? ngayHetHan,
        CancellationToken ct = default);

    /// <summary>Gia hạn 1 License có sẵn: cập nhật NgayHetHan mới, giữ nguyên MaLicenseKey,
    /// tự chuyển lại DangHoatDong nếu đang HetHan/TamKhoa.</summary>
    Task<LicenseDto?> RenewAsync(ulong id, DateOnly ngayHetHanMoi, CancellationToken ct = default);

    /// <summary>Khóa (TamKhoa) hoặc mở khóa (DangHoatDong) — không áp dụng cho License đã HetHan.</summary>
    Task<LicenseDto?> ToggleLockAsync(ulong id, bool khoa, CancellationToken ct = default);

    /// <summary>Tự động chuyển các License đã quá NgayHetHan sang HetHan — dùng cho background job,
    /// trả về số dòng đã chuyển.</summary>
    Task<int> MarkExpiredAsync(CancellationToken ct = default);

    /// <summary>Các License đang DangHoatDong sắp hết hạn trong N ngày tới — dùng để gửi email nhắc.</summary>
    Task<List<LicenseDto>> GetExpiringSoonAsync(int soNgayConLai, CancellationToken ct = default);
}
