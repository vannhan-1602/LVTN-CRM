namespace CRM.Infrastructure.Persistence.Entities;

public class KhKhachHang
{
    public ulong Id { get; set; }
    public string MaKhachHang { get; set; } = string.Empty;
    public string TenKhachHang { get; set; } = string.Empty;
    public ushort? LoaiKhachHangId { get; set; }
    public ushort? TinhTrangId { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string? MaSoThue { get; set; }
    public uint? NhanVienPhuTrachId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class KhLoaiKhachHang
{
    public ushort Id { get; set; }
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool IsActive { get; set; } = true;
}

public class KhTinhTrangKhachHang
{
    public ushort Id { get; set; }
    public string TenTinhTrang { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class SysAuditLog
{
    public ulong Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public ulong RecordId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public uint? UserId { get; set; }
    public DateTime? ChangedAt { get; set; }
}
