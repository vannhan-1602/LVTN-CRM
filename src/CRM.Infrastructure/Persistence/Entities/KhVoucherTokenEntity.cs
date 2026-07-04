using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Infrastructure.Persistence.Entities;

[Table("KH_Voucher_Token")]
public class KhVoucherTokenEntity
{
    public ulong Id { get; set; }
    public ulong Voucher_Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime NgayHetHanToken { get; set; }
    public bool DaSuDung { get; set; } = false;
    public DateTime? CreatedAt { get; set; }
}
