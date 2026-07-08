using CRM.Application.Models.Auth;

namespace CRM.Application.Interfaces.Auth;

public interface IUserRepository
{
    Task<UserAccount?> GetByUsernameWithRoleAsync(string username, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAccount>> GetAllWithRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>Lấy nhanh TokenVersion + trạng thái hiện tại — dùng cho middleware xác thực mỗi request.</summary>
    Task<(int TokenVersion, string TrangThai)?> GetTokenVersionAsync(uint userId, CancellationToken cancellationToken = default);

    /// <summary>Lấy tài khoản kèm mật khẩu (hash) theo Id — dùng cho chức năng tự đổi mật khẩu.</summary>
    Task<UserAccount?> GetByIdWithPasswordAsync(uint userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ghi nhận 1 lần đăng nhập sai. Sau 5 lần liên tiếp, khóa tạm tài khoản 15 phút
    /// (KhoaDenThoiDiem = giờ hiện tại + 15p). Trả về tổng số lần sai sau khi tăng.
    /// </summary>
    Task<int> GhiNhanDangNhapSaiAsync(uint userId, CancellationToken cancellationToken = default);

    /// <summary>Đăng nhập thành công: reset bộ đếm lần sai và bỏ khóa tạm (nếu có).</summary>
    Task ResetDangNhapSaiAsync(uint userId, CancellationToken cancellationToken = default);
}