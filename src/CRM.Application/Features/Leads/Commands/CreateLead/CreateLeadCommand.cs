using CRM.Application.Features.Leads.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.CreateLead
{
    public record CreateLeadCommand(
    string TenLead,
    string? TenCongTy,
    string? SoDienThoai,
    string? Email,
    uint? NhanVienPhuTrachId) : IRequest<LeadDto>;
}
