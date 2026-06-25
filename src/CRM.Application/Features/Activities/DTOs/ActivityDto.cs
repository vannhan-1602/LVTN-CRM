namespace CRM.Application.Features.Activities.DTOs;

public class ActivityDto
{
    public ulong Id { get; set; }
    public ulong? KhachHangId { get; set; }
    public string? TenKhachHang { get; set; }
    public ulong? LeadId { get; set; }
    public string? LoaiHoatDong { get; set; }
    public string? NoiDung { get; set; }
    public DateTime? ThoiGianThucHien { get; set; }
    public uint? NhanVienId { get; set; }
    public string? TenNhanVien { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateActivityRequestDto
{
    public ulong? KhachHangId { get; set; }
    public ulong? LeadId { get; set; }
    public string LoaiHoatDong { get; set; } = string.Empty;
    public string? NoiDung { get; set; }
    public DateTime ThoiGianThucHien { get; set; }
}

public class UpdateActivityRequestDto
{
    public string LoaiHoatDong { get; set; } = string.Empty;
    public string? NoiDung { get; set; }
    public DateTime ThoiGianThucHien { get; set; }
}