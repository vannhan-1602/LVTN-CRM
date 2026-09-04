using CRM.Application.Features.Users.DTOs;

namespace CRM.Application.Interfaces.Users;


public interface IUserManagementRepository
{
    Task<List<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(uint id, CancellationToken ct = default);

    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string? email, uint? excludeNhanSuId = null, CancellationToken ct = default);
    Task<bool> RoleExistsAsync(uint roleId, CancellationToken ct = default);
    Task<bool> PhongBanExistsAsync(ushort id, CancellationToken ct = default);
    Task<bool> ChucVuExistsAsync(ushort id, CancellationToken ct = default);

   
    Task<CreateUserValidationResult> ValidateNewUserAsync(
        string username, string? email, uint roleId, ushort? phongBanId, ushort? chucVuId,
        CancellationToken ct = default);

   
    Task<UpdateUserValidationResult> ValidateUserUpdateAsync(
        string? email, uint? excludeNhanSuId, uint roleId, ushort? phongBanId, ushort? chucVuId,
        CancellationToken ct = default);


    Task<uint> CreateAsync(
        string username, string passwordHash, uint roleId,
        string hoTen, string? email, string? soDienThoai,
        ushort? phongBanId, ushort? chucVuId,
        CancellationToken ct = default);

    Task UpdateAsync(
        uint userId, uint roleId,
        string hoTen, string? email, string? soDienThoai,
        ushort? phongBanId, ushort? chucVuId,
        CancellationToken ct = default);

    Task UpdatePasswordAsync(uint userId, string passwordHash, CancellationToken ct = default);
    Task UpdateStatusAsync(uint userId, string trangThai, CancellationToken ct = default);


    Task IncrementTokenVersionAsync(uint userId, CancellationToken ct = default);


    Task<bool> DeleteAsync(uint userId, CancellationToken ct = default);

    Task<UserLookupsDto> GetLookupsAsync(CancellationToken ct = default);
}


public record CreateUserValidationResult(
    bool UsernameAvailable, bool EmailAvailable, bool RoleValid, bool PhongBanValid, bool ChucVuValid);


public record UpdateUserValidationResult(
    bool EmailAvailable, bool RoleValid, bool PhongBanValid, bool ChucVuValid);