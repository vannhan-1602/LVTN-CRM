using CRM.Application.Features.Customers.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.ConvertLead
{
    public record ConvertLeadCommand(
    ulong LeadId,
    ushort? LoaiKhachHangId,
    ushort? TinhTrangKhachHangId) : IRequest<CustomerDto>;
}
