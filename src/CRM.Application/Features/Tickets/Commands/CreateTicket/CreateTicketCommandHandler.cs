using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Tickets.DTOs;
using CRM.Application.Features.Tickets.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
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
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogPublisher _auditLogPublisher;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<CreateTicketCommandHandler> _logger;

        public CreateTicketCommandHandler(
            ITicketRepository ticketRepository,
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            IAuditLogPublisher auditLogPublisher,
            ICurrentUserService currentUser,
            ILogger<CreateTicketCommandHandler> logger)
        {
            _ticketRepository = ticketRepository;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _auditLogPublisher = auditLogPublisher;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<TicketDto> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        {
            var khachHang = await _customerRepository.GetByIdAsync(request.KhachHangId, cancellationToken)
                ?? throw new NotFoundException(nameof(CRM.Domain.Entities.Customers.KhachHang), request.KhachHangId);

            // Sale chỉ được tạo Ticket cho Customer mình phụ trách 
            if (_currentUser.Role == Roles.Sale && khachHang.NhanVienPhuTrachId != _currentUser.NhanSuId)
                throw new ForbiddenException("Bạn chỉ có thể tạo ticket cho khách hàng mình phụ trách.");

            if (request.LoaiTicketId.HasValue &&
                !await _ticketRepository.LoaiTicketExistsAsync(request.LoaiTicketId.Value, cancellationToken))
                throw new BusinessRuleException("Loại ticket không hợp lệ hoặc đã bị khóa.");

            var maTicket = await _ticketRepository.GenerateMaTicketAsync(cancellationToken);

            var mucDoUuTien = string.IsNullOrWhiteSpace(request.MucDoUuTien)
                ? TicketPriority.TrungBinh
                : Enum.Parse<TicketPriority>(request.MucDoUuTien);

            var createdAt = DateTime.UtcNow;
            var soGioXuLy = await _ticketRepository.GetSlaSoGioXuLyAsync(mucDoUuTien.ToString(), cancellationToken);
            var thoiHanSla = soGioXuLy.HasValue ? createdAt.AddHours(soGioXuLy.Value) : (DateTime?)null;

            // Sale tạo ticket -> LUÔN là người tiếp nhận và người xử lý mặc định
            // Manager được toàn quyền chỉ định theo request.
            var nhanVienTiepNhanId = _currentUser.Role == Roles.Sale
                ? _currentUser.NhanSuId
                : request.NhanVienTiepNhanId;
            var nhanVienXuLyId = _currentUser.Role == Roles.Sale
                ? _currentUser.NhanSuId
                : request.NhanVienXuLyId;

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
                MucDoUuTien = mucDoUuTien,
                NguonTiepNhan = string.IsNullOrWhiteSpace(request.NguonTiepNhan)
                    ? TicketSource.Phone
                    : Enum.Parse<TicketSource>(request.NguonTiepNhan),
                TrangThai = TicketStatus.Moi,
                NhanVienTiepNhanId = nhanVienTiepNhanId,
                NhanVienXuLyId = nhanVienXuLyId,
                NgayHenXuLy = request.NgayHenXuLy,
                ThoiHanSLA = thoiHanSla,
                IsDeleted = false,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
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
