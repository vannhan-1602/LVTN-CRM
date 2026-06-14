using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Tickets.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Tickets;
using CRM.Domain.Entities.Tickets;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Tickets.Commands.DeleteTicket
{
    public class DeleteTicketCommandHandler : IRequestHandler<DeleteTicketCommand, bool>
    {
        private const string AuditTable = "TK_Ticket";
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogPublisher _auditLogPublisher;
        private readonly ILogger<DeleteTicketCommandHandler> _logger;

        public DeleteTicketCommandHandler(
            ITicketRepository ticketRepository, IUnitOfWork unitOfWork,
            IAuditLogPublisher auditLogPublisher, ILogger<DeleteTicketCommandHandler> logger)
        {
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
            _auditLogPublisher = auditLogPublisher;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteTicketCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Ticket), request.Id);

            var oldDto = TicketMapper.ToDto(ticket);

            var deleted = await _ticketRepository.DeleteAsync(request.Id, cancellationToken);
            if (!deleted) throw new NotFoundException(nameof(Ticket), request.Id);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "DELETE",
                    oldData: oldDto, newData: null, cancellationToken);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for ticket {Id}", request.Id); }

            return true;
        }
    }
}