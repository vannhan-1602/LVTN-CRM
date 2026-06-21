using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Tickets.DTOs;
using CRM.Application.Features.Tickets.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Tickets;
using CRM.Domain.Entities.Tickets;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Tickets.Commands.AssignTicket
{
    /// <summary>
    /// Gán (hoặc chuyển) nhân viên xử lý cho ticket. Nếu ticket đang ở trạng thái "Mới" sẽ
    /// tự động chuyển sang "Đang xử lý" và ghi nhận một bản phản hồi nội bộ.
    ///
    /// Quy tắc quyền hạn: Sale chỉ được thao tác trên ticket mình đang phụ trách
    /// (tự nhận hoặc cập nhật ticket của chính mình) — việc điều phối/chuyển ticket
    /// cho nhân viên khác do Manager thực hiện (Manager không bị giới hạn ở đây).
    /// </summary>
    public class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand, TicketDto>
    {
        private const string AuditTable = "TK_Ticket";
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogPublisher _auditLogPublisher;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<AssignTicketCommandHandler> _logger;

        public AssignTicketCommandHandler(
            ITicketRepository ticketRepository,
            IUnitOfWork unitOfWork,
            IAuditLogPublisher auditLogPublisher,
            ICurrentUserService currentUser,
            ILogger<AssignTicketCommandHandler> logger)
        {
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
            _auditLogPublisher = auditLogPublisher;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<TicketDto> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Ticket), request.Id);

            if (_currentUser.Role == Roles.Sale)
            {
                // Sale chỉ được thao tác trên ticket hiện đang là của mình (hoặc chưa
                // ai nhận — NhanVienXuLyId null) và chỉ được tự gán cho chính mình,
                // không được chuyển ticket cho đồng nghiệp khác.
                var isOwnedByMeOrUnassigned = ticket.NhanVienXuLyId == null || ticket.NhanVienXuLyId == _currentUser.NhanSuId;
                if (!isOwnedByMeOrUnassigned)
                    throw new ForbiddenException("Bạn không có quyền thao tác trên ticket của nhân viên khác.");

                if (request.NhanVienXuLyId != _currentUser.NhanSuId)
                    throw new ForbiddenException("Bạn chỉ có thể tự nhận xử lý ticket, không thể gán cho người khác.");
            }

            if (ticket.TrangThai == TicketStatus.Dong)
                throw new BusinessRuleException("Ticket đã đóng, không thể gán xử lý.");

            var oldDto = TicketMapper.ToDto(ticket);
            var trangThaiTruoc = ticket.TrangThai;

            ticket.NhanVienXuLyId = request.NhanVienXuLyId;
            if (request.NgayHenXuLy.HasValue)
                ticket.NgayHenXuLy = request.NgayHenXuLy;

            if (ticket.TrangThai == TicketStatus.Moi)
                ticket.TrangThai = TicketStatus.DangXuLy;

            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket, cancellationToken);

            await _ticketRepository.AddPhanHoiAsync(new TicketPhanHoi
            {
                TicketId = ticket.Id,
                NguoiPhanHoiId = request.NhanVienXuLyId,
                LoaiPhanHoi = TicketPhanHoiLoai.NoiBoXuLy,
                NoiDung = $"Ticket được gán cho nhân viên xử lý (Id: {request.NhanVienXuLyId}).",
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
