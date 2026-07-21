using CRM.Domain.Common;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities.Customers;

public class Lead
{
    public ulong Id { get; set; }
    public string TenLead { get; set; } = string.Empty;
    public string? TenCongTy { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string? NguonLead { get; set; } = "Manual"; // Thêm dòng này vào class Lead trong Domain
    public string? TinhTrang { get; set; }
    public uint? NhanVienPhuTrachId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}