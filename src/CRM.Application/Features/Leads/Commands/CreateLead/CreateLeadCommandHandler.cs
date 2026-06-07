using CRM.Application.Features.Leads.DTOs;
using CRM.Application.Features.Leads.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Leads;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.CreateLead
{
    public class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, LeadDto>
    {
        private const string AuditTable = "KH_Lead";
        private readonly ILeadRepository _leadRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogPublisher _auditLogPublisher;
        private readonly ILogger<CreateLeadCommandHandler> _logger;

        public CreateLeadCommandHandler(
            ILeadRepository leadRepository,
            IUnitOfWork unitOfWork,
            IAuditLogPublisher auditLogPublisher,
            ILogger<CreateLeadCommandHandler> logger)
        {
            _leadRepository = leadRepository;
            _unitOfWork = unitOfWork;
            _auditLogPublisher = auditLogPublisher;
            _logger = logger;
        }

        public async Task<LeadDto> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
        {
            var lead = new Lead
            {
                TenLead = request.TenLead.Trim(),
                TenCongTy = request.TenCongTy?.Trim(),
                SoDienThoai = request.SoDienThoai?.Trim(),
                Email = request.Email?.Trim(),
                TinhTrang = LeadTinhTrang.Moi,
                NhanVienPhuTrachId = request.NhanVienPhuTrachId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _leadRepository.AddAsync(lead, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = LeadMapper.ToDto(created);
            try
            {
                await _auditLogPublisher.PublishAsync(AuditTable, created.Id, "INSERT",
                    oldData: null, newData: dto, cancellationToken);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for lead {Id}", created.Id); }

            return dto;
        }
    }
}
