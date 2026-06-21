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

namespace CRM.Application.Features.Tickets.Commands.AddPhanHoi
{
    /// Thêm phản hồi cho ticket. Tùy loại phản hồi, trạng thái ticket sẽ tự động chuyển:
    ///  - PhanHoiKhachHang  -> ChoPhanHoi
    ///  - YeuCauBoSung      -> ChoPhanHoi
    ///  - NoiBoXuLy         -> DangXuLy (nếu đang Mới/ChoPhanHoi)
    ///  - DongTicket        -> Dong 
    public class AddPhanHoiCommandHandler : IRequestHandler<AddPhanHoiCommand, TicketPhanHoiDto>
    {
        private const string AuditTable = "TK_Ticket_PhanHoi";
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogPublisher _auditLogPublisher;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<AddPhanHoiCommandHandler> _logger;

        public AddPhanHoiCommandHandler(
            ITicketRepository ticketRepository,
            IUnitOfWork unitOfWork,
            IAuditLogPublisher auditLogPublisher,
            ICurrentUserService currentUser,
            ILogger<AddPhanHoiCommandHandler> logger)
        {
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
            _auditLogPublisher = auditLogPublisher;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<TicketPhanHoiDto> Handle(AddPhanHoiCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
                ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

            // Chặn Sale phản hồi vào Ticket không phải mình xử lý
            if (_currentUser.Role == Roles.Sale && ticket.NhanVienXuLyId != _currentUser.NhanSuId)
                throw new ForbiddenException("Bạn không có quyền thao tác trên ticket của nhân viên khác.");

            if (ticket.TrangThai == TicketStatus.Dong)
                throw new BusinessRuleException("Ticket đã đóng, không thể thêm phản hồi.");

            var loaiPhanHoi = Enum.Parse<TicketPhanHoiLoai>(request.LoaiPhanHoi);
            var trangThaiTruoc = ticket.TrangThai;

            switch (loaiPhanHoi)
            {
                case TicketPhanHoiLoai.PhanHoiKhachHang:
                case TicketPhanHoiLoai.YeuCauBoSung:
                    ticket.TrangThai = TicketStatus.ChoPhanHoi;
                    break;
                case TicketPhanHoiLoai.NoiBoXuLy:
                    if (ticket.TrangThai is TicketStatus.Moi or TicketStatus.ChoPhanHoi)
                        ticket.TrangThai = TicketStatus.DangXuLy;
                    break;
                case TicketPhanHoiLoai.DongTicket:
                    ticket.TrangThai = TicketStatus.Dong;
                    ticket.NgayDong = DateTime.UtcNow;
                    break;
            }

            ticket.UpdatedAt = DateTime.UtcNow;
            await _ticketRepository.UpdateAsync(ticket, cancellationToken);

            var phanHoi = new TicketPhanHoi
            {
                TicketId = ticket.Id,
                NguoiPhanHoiId = request.NguoiPhanHoiId,
                LoaiPhanHoi = loaiPhanHoi,
                NoiDung = request.NoiDung.Trim(),
                FileDinhKem = request.FileDinhKem,
                TrangThaiTruoc = trangThaiTruoc,
                TrangThaiSau = ticket.TrangThai,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _ticketRepository.AddPhanHoiAsync(phanHoi, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = TicketMapper.ToDto(created);
            try
            {
                await _auditLogPublisher.PublishAsync(AuditTable, created.Id, "INSERT",
                    oldData: null, newData: dto, cancellationToken);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for ticket phanhoi {Id}", created.Id); }

            return dto;
        }
    }
}
