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

    /// Tạo HT_ThongTinNhanSu + HT_User trong cùng 1 transaction nghiệp vụ
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

    /// <summary>
    /// Tăng TokenVersion — vô hiệu hóa mọi JWT đã phát trước đó cho tài khoản này ngay lập tức
    /// (khóa/vô hiệu hóa, đổi vai trò, đổi mật khẩu). Xem thêm TokenVersionCacheService.
    /// </summary>
    Task IncrementTokenVersionAsync(uint userId, CancellationToken ct = default);

    /// Xóa cứng tài khoản (giữ lại bản ghi nhân sự, chỉ gỡ liên kết đăng nhập).
    Task<bool> DeleteAsync(uint userId, CancellationToken ct = default);

    Task<UserLookupsDto> GetLookupsAsync(CancellationToken ct = default);
}