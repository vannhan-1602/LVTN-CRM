using CRM.Application.Interfaces.Leads;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.CreatePublicLead;

public class CreatePublicLeadCommandHandler : IRequestHandler<CreatePublicLeadCommand, ulong>
{
    private readonly ILeadRepository _leadRepository;

    public CreatePublicLeadCommandHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<ulong> Handle(CreatePublicLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = new Lead
        {
            TenLead = request.TenLead.Trim(),
            TenCongTy = request.TenCongTy?.Trim(),
            SoDienThoai = request.SoDienThoai?.Trim(),
            Email = request.Email?.Trim(),
            NguonLead = "Website", // Set cứng nguồn từ Landing Page là Website
            TinhTrang = LeadTinhTrang.Moi,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _leadRepository.AddAsync(lead, cancellationToken);
        return created.Id;
    }
}