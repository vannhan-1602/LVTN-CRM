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

namespace CRM.Application.Features.Tickets.Commands.CloseTicket
{
  
    /// Đóng ticket: chuyển trạng thái sang "Dong", ghi nhận ngày đóng, lý do đóng
    /// và thêm một bản phản hồi loại DongTicket.

    public class CloseTicketCommandHandler : IRequestHandler<CloseTicketCommand, TicketDto>
    {
        private const string AuditTable = "TK_Ticket";
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogPublisher _auditLogPublisher;
        private readonly ILogger<CloseTicketCommandHandler> _logger;

        public CloseTicketCommandHandler(
            ITicketRepository ticketRepository,
            IUnitOfWork unitOfWork,
            IAuditLogPublisher auditLogPublisher,
            ILogger<CloseTicketCommandHandler> logger)
        {
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
            _auditLogPublisher = auditLogPublisher;
            _logger = logger;
        }

        public async Task<TicketDto> Handle(CloseTicketCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Ticket), request.Id);

            if (ticket.TrangThai == TicketStatus.Dong)
                throw new BusinessRuleException("Ticket đã đóng trước đó.");

            var oldDto = TicketMapper.ToDto(ticket);
            var trangThaiTruoc = ticket.TrangThai;

            ticket.TrangThai = TicketStatus.Dong;
            ticket.NgayDong = DateTime.UtcNow;
            ticket.LyDoDong = request.LyDoDong?.Trim();
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket, cancellationToken);

            await _ticketRepository.AddPhanHoiAsync(new TicketPhanHoi
            {
                TicketId = ticket.Id,
                NguoiPhanHoiId = request.NguoiDongId,
                LoaiPhanHoi = TicketPhanHoiLoai.DongTicket,
                NoiDung = string.IsNullOrWhiteSpace(request.LyDoDong)
                    ? "Ticket đã được đóng."
                    : $"Ticket đã được đóng. Lý do: {request.LyDoDong.Trim()}",
                TrangThaiTruoc = trangThaiTruoc,
                TrangThaiSau = ticket.TrangThai,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

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
