using CRM.Application.Interfaces.Auth;

namespace CRM.Infrastructure.Identity;


/// Xác thực mật khẩu. Hỗ trợ plain-text (dữ liệu DB cũ) và BCrypt hash (prefix $2).
/// Mật khẩu mới luôn được băm bằng BCrypt khi tạo/đặt lại.

public class PasswordHasher : IPasswordHasher
{
    public bool Verify(string password, string storedPassword)
    {
        if (string.IsNullOrEmpty(storedPassword))
        {
            return false;
        }

        if (storedPassword.StartsWith("$2", StringComparison.Ordinal))
        {
            return BCrypt.Net.BCrypt.Verify(password, storedPassword);
        }

        return string.Equals(password, storedPassword, StringComparison.Ordinal);
    }

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
}