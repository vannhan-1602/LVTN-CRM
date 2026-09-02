namespace CRM.Application.Features.Auth.DTOs;

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Response trả về CHO CLIENT — không bao giờ chứa refresh token (refresh token chỉ đi qua
// cookie HttpOnly, không lộ ra JSON body để giảm rủi ro XSS đánh cắp token).
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

// Kết quả nội bộ (MediatR handler -> Controller). Chứa thêm refresh token dạng plaintext —
// Controller lấy ra để set cookie HttpOnly rồi bỏ đi, KHÔNG bao giờ serialize field này ra
// response JSON (Controller luôn map sang LoginResponseDto trước khi trả về client).
public class LoginResultDto : LoginResponseDto
{
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}

// Tương tự RefreshResultDto: NewRefreshToken chỉ dùng nội bộ để Controller set cookie,
// không bao giờ trả field này ra JSON body.
public class RefreshResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string NewRefreshToken { get; set; } = string.Empty;
    public DateTime NewRefreshTokenExpiresAt { get; set; }
}

// Response an toàn trả về client cho endpoint /refresh (không chứa refresh token).
public class AccessTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
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
