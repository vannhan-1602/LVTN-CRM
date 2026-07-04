using CRM.Domain.Common;

namespace CRM.Domain.Entities.Loyalty;

public class VoucherToken : BaseEntity<ulong>
{
    public ulong VoucherId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime NgayHetHanToken { get; set; }
    public bool DaSuDung { get; set; } = false;
    public DateTime? CreatedAt { get; set; }
}
