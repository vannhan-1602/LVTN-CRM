using CRM.Application.Interfaces.Auth;

namespace CRM.Infrastructure.Identity;

/// <summary>
/// Xác thực mật khẩu. Hỗ trợ plain-text (DB hiện tại) và BCrypt hash (prefix $2).
/// </summary>
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
}
