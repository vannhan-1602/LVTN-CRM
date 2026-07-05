using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Leads;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Leads.Commands.RestoreLead
{
    public class RestoreLeadCommandHandler : IRequestHandler<RestoreLeadCommand, bool>
    {
        private const string AuditTable = "KH_Lead";
        private readonly ILeadRepository _leadRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogPublisher _auditLogPublisher;
        private readonly ILogger<RestoreLeadCommandHandler> _logger;

        public RestoreLeadCommandHandler(
            ILeadRepository leadRepository, IUnitOfWork unitOfWork,
            IAuditLogPublisher auditLogPublisher, ILogger<RestoreLeadCommandHandler> logger)
        {
            _leadRepository = leadRepository;
            _unitOfWork = unitOfWork;
            _auditLogPublisher = auditLogPublisher;
            _logger = logger;
        }

        public async Task<bool> Handle(RestoreLeadCommand request, CancellationToken cancellationToken)
        {
            var restored = await _leadRepository.RestoreAsync(request.Id, cancellationToken);
            if (!restored) throw new NotFoundException(nameof(Lead), request.Id);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "RESTORE",
                    oldData: new { IsDeleted = true }, newData: new { IsDeleted = false }, cancellationToken);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for restored lead {Id}", request.Id); }

            return true;
        }
    }
}
