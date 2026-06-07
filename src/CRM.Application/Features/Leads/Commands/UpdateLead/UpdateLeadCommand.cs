using CRM.Application.Features.Leads.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.UpdateLead
{
    public record UpdateLeadCommand(
    ulong Id,
    string TenLead,
    string? TenCongTy,
    string? SoDienThoai,
    string? Email,
    string? TinhTrang,
    uint? NhanVienPhuTrachId) : IRequest<LeadDto>;
}
