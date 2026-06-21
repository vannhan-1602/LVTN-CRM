namespace CRM.Infrastructure.Persistence.Entities;

public class HtRole
{
    public uint Id { get; set; }
    public string TenRole { get; set; } = string.Empty;
    public string? MoTa { get; set; }

    public ICollection<HtUser> Users { get; set; } = [];
}

public class HtThongTinNhanSu
{
    public uint Id { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public ushort? PhongBanId { get; set; }
    public ushort? ChucVuId { get; set; }
    public bool TrangThai { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HtUser? User { get; set; }

    public HtPhongBan? PhongBan { get; set; }
    public HtChucVu? ChucVu { get; set; }
}

public class HtUser
{
    public uint Id { get; set; }
    public uint? NhanSuId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public uint? RoleId { get; set; }
    public string TrangThai { get; set; } = "Active";
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HtThongTinNhanSu? NhanSu { get; set; }
    public HtRole? Role { get; set; }
}
