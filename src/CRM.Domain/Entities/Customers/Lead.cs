using CRM.Domain.Common;
using CRM.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities.Customers
{
    public class Lead : AuditableEntity<ulong>
    {
        public string TenLead { get; set; } = string.Empty;
        public string? TenCongTy { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string TinhTrang { get; set; } = LeadTinhTrang.Moi;
        public uint? NhanVienPhuTrachId { get; set; }
    }
}
