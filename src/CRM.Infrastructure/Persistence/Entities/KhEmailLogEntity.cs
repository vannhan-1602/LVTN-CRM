using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_EmailLog")]
public class KhEmailLogEntity
{
    public ulong Id { get; set; }
    public ulong KhachHang_Id { get; set; }
    public string LoaiEmail { get; set; } = string.Empty;
    public ulong? Voucher_Id { get; set; }
    public string EmailDen { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public string TrangThaiGui { get; set; } = "ThanhCong";
    public string? LoiChiTiet { get; set; }
    public DateTime? CreatedAt { get; set; }
}
