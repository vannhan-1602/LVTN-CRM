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

namespace CRM.Application.Features.Tickets.Commands.CreateTicket
{
    public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, TicketDto>
    {
        private const string AuditTable = "TK_Ticket";
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogPublisher _auditLogPublisher;
        private readonly ILogger<CreateTicketCommandHandler> _logger;

        public CreateTicketCommandHandler(
            ITicketRepository ticketRepository,
            IUnitOfWork unitOfWork,
            IAuditLogPublisher auditLogPublisher,
            ILogger<CreateTicketCommandHandler> logger)
        {
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
            _auditLogPublisher = auditLogPublisher;
            _logger = logger;
        }

        public async Task<TicketDto> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        {
            if (!await _ticketRepository.KhachHangExistsAsync(request.KhachHangId, cancellationToken))
                throw new NotFoundException(nameof(CRM.Domain.Entities.Customers.KhachHang), request.KhachHangId);

            if (request.LoaiTicketId.HasValue &&
                !await _ticketRepository.LoaiTicketExistsAsync(request.LoaiTicketId.Value, cancellationToken))
                throw new BusinessRuleException("Loại ticket không hợp lệ hoặc đã bị khóa.");

            var maTicket = await _ticketRepository.GenerateMaTicketAsync(cancellationToken);

            var ticket = new Ticket
            {
                MaTicket = maTicket,
                TieuDe = request.TieuDe.Trim(),
                MoTa = request.MoTa?.Trim(),
                FileDinhKem = request.FileDinhKem,
                LoaiTicketId = request.LoaiTicketId,
                KhachHangId = request.KhachHangId,
                HopDongId = request.HopDongId,
                SanPhamId = request.SanPhamId,
                MucDoUuTien = string.IsNullOrWhiteSpace(request.MucDoUuTien)
                    ? TicketPriority.TrungBinh
                    : Enum.Parse<TicketPriority>(request.MucDoUuTien),
                NguonTiepNhan = string.IsNullOrWhiteSpace(request.NguonTiepNhan)
                    ? TicketSource.Phone
                    : Enum.Parse<TicketSource>(request.NguonTiepNhan),
                TrangThai = TicketStatus.Moi,
                NhanVienTiepNhanId = request.NhanVienTiepNhanId,
                NhanVienXuLyId = request.NhanVienXuLyId,
                NgayHenXuLy = request.NgayHenXuLy,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _ticketRepository.AddAsync(ticket, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = TicketMapper.ToDto(created);
            try
            {
                await _auditLogPublisher.PublishAsync(AuditTable, created.Id, "INSERT",
                    oldData: null, newData: dto, cancellationToken);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for ticket {Id}", created.Id); }

            return dto;
        }
    }
}