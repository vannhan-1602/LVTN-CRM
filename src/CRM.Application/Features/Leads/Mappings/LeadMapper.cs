using CRM.Application.Features.Leads.DTOs;
using CRM.Domain.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Mappings
{
    public static class LeadMapper
    {
        public static LeadDto ToDto(Lead lead) => new()
        {
            Id = lead.Id,
            TenLead = lead.TenLead,
            TenCongTy = lead.TenCongTy,
            SoDienThoai = lead.SoDienThoai,
            Email = lead.Email,
            TinhTrang = lead.TinhTrang,
            NhanVienPhuTrachId = lead.NhanVienPhuTrachId,
            IsDeleted = lead.IsDeleted,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt
        };
    }
}
