using CRM.Application.Features.Addresses.DTOs;

namespace CRM.Application.Interfaces.Addresses;

public interface IAddressRepository
{
    Task<List<AddressDto>> GetByKhachHangAsync(ulong khachHangId, CancellationToken ct = default);
    Task<AddressDto?> GetByIdAsync(ulong id, CancellationToken ct = default);
    Task<AddressDto> AddAsync(ulong khachHangId, string? diaChiChiTiet, uint? tinhThanhId, uint? phuongXaId, string loaiDiaChi, bool isDefault, CancellationToken ct = default);
    Task<AddressDto> UpdateAsync(ulong id, string? diaChiChiTiet, uint? tinhThanhId, uint? phuongXaId, string loaiDiaChi, bool isDefault, CancellationToken ct = default);
    Task<bool> DeleteAsync(ulong id, CancellationToken ct = default);
}