namespace CRM.Application.Features.Auth.DTOs;

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public uint UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? HoTen { get; set; }
    public string? Email { get; set; }

    public uint? NhanSuId { get; set; }
}

public class UserSummaryDto
{
    public uint Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string? HoTen { get; set; }
    public string? Email { get; set; }
    public DateTime? CreatedAt { get; set; }
}
