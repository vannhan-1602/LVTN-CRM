using CRM.Application.Features.Activities.DTOs;

namespace CRM.Application.Interfaces.Activities;

public interface IActivityRepository
{
    Task<List<ActivityDto>> GetByKhachHangAsync(ulong khachHangId, CancellationToken ct = default);
    Task<List<ActivityDto>> GetByLeadAsync(ulong leadId, CancellationToken ct = default);
    Task<ActivityDto?> GetByIdAsync(ulong id, CancellationToken ct = default);

    Task<ActivityDto> AddAsync(
        ulong? khachHangId,
        ulong? leadId,
        string loaiHoatDong,
        string? noiDung,
        DateTime thoiGianThucHien,
        uint? nhanVienId,
        CancellationToken ct = default);

    Task<ActivityDto> UpdateAsync(
        ulong id,
        string loaiHoatDong,
        string? noiDung,
        DateTime thoiGianThucHien,
        CancellationToken ct = default);

    Task<bool> DeleteAsync(ulong id, CancellationToken ct = default);
}