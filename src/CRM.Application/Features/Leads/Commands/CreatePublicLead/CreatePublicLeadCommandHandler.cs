using CRM.Application.Interfaces.Leads;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.CreatePublicLead;

public class CreatePublicLeadCommandHandler : IRequestHandler<CreatePublicLeadCommand, ulong>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePublicLeadCommandHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
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
        // BUG cũ: thiếu dòng này nên EF chỉ track entity trong bộ nhớ rồi vứt luôn khi request kết thúc —
        // API trả về "thành công" nhưng KHÔNG có dòng nào được ghi xuống DB (Id lead cũng luôn = 0).
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return created.Id;
    }
}