using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Tickets.DTOs;
using CRM.Application.Features.Tickets.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Tickets;
using CRM.Domain.Entities.Tickets;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Tickets.Commands.UpdateTicket
{
    public class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand, TicketDto>
    {
        private const string AuditTable = "TK_Ticket";
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogPublisher _auditLogPublisher;
        private readonly ILogger<UpdateTicketCommandHandler> _logger;

        public UpdateTicketCommandHandler(
            ITicketRepository ticketRepository,
            IUnitOfWork unitOfWork,
            IAuditLogPublisher auditLogPublisher,
            ILogger<UpdateTicketCommandHandler> logger)
        {
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
            _auditLogPublisher = auditLogPublisher;
            _logger = logger;
        }

        public async Task<TicketDto> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Ticket), request.Id);

            if (ticket.TrangThai == TicketStatus.Dong)
                throw new BusinessRuleException("Ticket đã đóng, không thể cập nhật.");

            if (request.LoaiTicketId.HasValue &&
                !await _ticketRepository.LoaiTicketExistsAsync(request.LoaiTicketId.Value, cancellationToken))
                throw new BusinessRuleException("Loại ticket không hợp lệ hoặc đã bị khóa.");

            var oldDto = TicketMapper.ToDto(ticket);

            ticket.TieuDe = request.TieuDe.Trim();
            ticket.MoTa = request.MoTa?.Trim();
            ticket.FileDinhKem = request.FileDinhKem;
            ticket.LoaiTicketId = request.LoaiTicketId;
            ticket.HopDongId = request.HopDongId;
            ticket.SanPhamId = request.SanPhamId;

            if (!string.IsNullOrWhiteSpace(request.MucDoUuTien))
                ticket.MucDoUuTien = Enum.Parse<TicketPriority>(request.MucDoUuTien);

            if (!string.IsNullOrWhiteSpace(request.NguonTiepNhan))
                ticket.NguonTiepNhan = Enum.Parse<TicketSource>(request.NguonTiepNhan);

            if (!string.IsNullOrWhiteSpace(request.TrangThai))
                ticket.TrangThai = Enum.Parse<TicketStatus>(request.TrangThai);

            ticket.NgayHenXuLy = request.NgayHenXuLy;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var newDto = TicketMapper.ToDto(ticket);
            try
            {
                await _auditLogPublisher.PublishAsync(AuditTable, ticket.Id, "UPDATE",
                    oldData: oldDto, newData: newDto, cancellationToken);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for ticket {Id}", ticket.Id); }

            return newDto;
        }
    }
}