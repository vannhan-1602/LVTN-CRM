using CRM.Application.Common.Exceptions;
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

namespace CRM.Application.Features.Leads.Commands.DeleteLead
{
    public class DeleteLeadCommandHandler : IRequestHandler<DeleteLeadCommand, bool>
    {
        private const string AuditTable = "KH_Lead";
        private readonly ILeadRepository _leadRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogPublisher _auditLogPublisher;
        private readonly ILogger<DeleteLeadCommandHandler> _logger;

        public DeleteLeadCommandHandler(
            ILeadRepository leadRepository, IUnitOfWork unitOfWork,
            IAuditLogPublisher auditLogPublisher, ILogger<DeleteLeadCommandHandler> logger)
        {
            _leadRepository = leadRepository;
            _unitOfWork = unitOfWork;
            _auditLogPublisher = auditLogPublisher;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
        {
            var lead = await _leadRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken)
                ?? throw new NotFoundException(nameof(Lead), request.Id);

            var oldDto = LeadMapper.ToDto(lead);

            var deleted = await _leadRepository.DeleteAsync(request.Id, cancellationToken);
            if (!deleted) throw new NotFoundException(nameof(Lead), request.Id);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "DELETE",
                    oldData: oldDto, newData: null, cancellationToken);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for lead {Id}", request.Id); }

            return true;
        }
    }

}
