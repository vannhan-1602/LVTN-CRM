using CRM.Application.Features.Contracts.DTOs;

namespace CRM.Application.Interfaces.Contracts;

/// <summary>Quản lý mốc triển khai (HD_MocTrienKhai): Đào tạo / Bàn giao / Nghiệm thu của 1 hợp đồng.
/// Cho phép nhiều dòng cùng LoaiMoc=DaoTao (mỗi buổi đào tạo 1 dòng) để đối chiếu "đã đào tạo x/y buổi".</summary>
public interface IContractMilestoneRepository
{
    Task<List<MocTrienKhaiDto>> GetByHopDongAsync(ulong hopDongId, CancellationToken ct = default);
    Task<MocTrienKhaiDto?> GetByIdAsync(ulong id, CancellationToken ct = default);
    Task<MocTrienKhaiDto> AddAsync(ulong hopDongId, string loaiMoc, string? noiDung,
        DateTime? ngayThucHien, uint? nhanVienThucHienId, CancellationToken ct = default);
    Task<MocTrienKhaiDto?> UpdateAsync(ulong id, string? noiDung, DateTime? ngayThucHien,
        uint? nhanVienThucHienId, string? nguoiXacNhanKhach, string? fileBienBan, string trangThai,
        CancellationToken ct = default);
    Task<bool> DeleteAsync(ulong id, CancellationToken ct = default);
}
