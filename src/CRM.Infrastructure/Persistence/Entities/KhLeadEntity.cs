using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Persistence.Entities
{
    [Table("KH_Lead")]
    public class KhLeadEntity
    {
        public ulong Id { get; set; }
        public string TenLead { get; set; } = string.Empty;
        public string? TenCongTy { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? TinhTrang { get; set; }
        public uint? NhanVienPhuTrach_Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
