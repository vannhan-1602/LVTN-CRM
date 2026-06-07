using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Leads.DTOs;
using CRM.Application.Features.Leads.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Leads;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.UpdateLead
{
    public class UpdateLeadCommandHandler : IRequestHandler<UpdateLeadCommand, LeadDto>
    {
        private const string AuditTable = "KH_Lead";
        private readonly ILeadRepository _leadRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogPublisher _auditLogPublisher;
        private readonly ILogger<UpdateLeadCommandHandler> _logger;

        public UpdateLeadCommandHandler(
            ILeadRepository leadRepository, IUnitOfWork unitOfWork,
            IAuditLogPublisher auditLogPublisher, ILogger<UpdateLeadCommandHandler> logger)
        {
            _leadRepository = leadRepository;
            _unitOfWork = unitOfWork;
            _auditLogPublisher = auditLogPublisher;
            _logger = logger;
        }

        public async Task<LeadDto> Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
        {
            var lead = await _leadRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Lead), request.Id);

            var oldDto = LeadMapper.ToDto(lead);

            lead.TenLead = request.TenLead.Trim();
            lead.TenCongTy = request.TenCongTy?.Trim();
            lead.SoDienThoai = request.SoDienThoai?.Trim();
            lead.Email = request.Email?.Trim();
            if (!string.IsNullOrWhiteSpace(request.TinhTrang))
                lead.TinhTrang = request.TinhTrang;
            lead.NhanVienPhuTrachId = request.NhanVienPhuTrachId;
            lead.UpdatedAt = DateTime.UtcNow;

            await _leadRepository.UpdateAsync(lead, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var newDto = LeadMapper.ToDto(lead);
            try
            {
                await _auditLogPublisher.PublishAsync(AuditTable, lead.Id, "UPDATE",
                    oldData: oldDto, newData: newDto, cancellationToken);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for lead {Id}", lead.Id); }

            return newDto;
        }
    }
}
