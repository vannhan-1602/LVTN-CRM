namespace CRM.Application.Features.Users.DTOs;

public class UserDto
{
    public uint Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;   // Active | Locked | Inactive

    public uint? RoleId { get; set; }
    public string? RoleName { get; set; }

    public uint? NhanSuId { get; set; }
    public string? HoTen { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }

    public ushort? PhongBanId { get; set; }
    public string? TenPhongBan { get; set; }

    public ushort? ChucVuId { get; set; }
    public string? TenChucVu { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateUserRequestDto
{
    // Tài khoản đăng nhập
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public uint RoleId { get; set; }

    // Thông tin nhân sự (tạo kèm — quan hệ 1-1 với HT_User)
    public string HoTen { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public ushort? PhongBanId { get; set; }
    public ushort? ChucVuId { get; set; }
}

public class UpdateUserRequestDto
{
    // Không cho đổi Username  — chỉ đổi role + thông tin nhân sự
    public uint RoleId { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public ushort? PhongBanId { get; set; }
    public ushort? ChucVuId { get; set; }
}

public class ResetPasswordRequestDto
{
    public string NewPassword { get; set; } = string.Empty;
}

public class ToggleUserStatusRequestDto
{
    // Active | Locked | Inactive
    public string TrangThai { get; set; } = string.Empty;
}

public class PhongBanDto
{
    public ushort Id { get; set; }
    public string TenPhongBan { get; set; } = string.Empty;
}

public class ChucVuDto
{
    public ushort Id { get; set; }
    public string TenChucVu { get; set; } = string.Empty;
}

public class RoleDto
{
    public uint Id { get; set; }
    public string TenRole { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}

public class UserLookupsDto
{
    public List<RoleDto> Roles { get; set; } = [];
    public List<PhongBanDto> PhongBans { get; set; } = [];
    public List<ChucVuDto> ChucVus { get; set; } = [];
}
