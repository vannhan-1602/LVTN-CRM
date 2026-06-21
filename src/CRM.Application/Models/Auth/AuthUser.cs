namespace CRM.Application.Models.Auth;

public class AuthUser
{
    public uint Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? HoTen { get; set; }
    public string? Email { get; set; }

 
    public uint? NhanSuId { get; set; }
}

public class AuthTokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class UserAccount
{
    public uint Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public uint? RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public uint? NhanSuId { get; set; }   
    public string? HoTen { get; set; }
    public string? Email { get; set; }
    public DateTime? CreatedAt { get; set; }
}
