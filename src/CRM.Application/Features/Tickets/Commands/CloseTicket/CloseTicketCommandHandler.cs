using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Tickets.DTOs;
using CRM.Application.Features.Tickets.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Email;
using CRM.Application.Interfaces.Tickets;
using CRM.Domain.Entities.Tickets;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Tickets.Commands.CloseTicket;

// Đóng ticket: chuyển trạng thái sang "Dong", ghi nhận ngày đóng, lý do đóng
// và thêm một bản phản hồi loại DongTicket. Sau đó tạo yêu cầu khảo sát hài lòng
// (CSAT) và gửi email kèm link công khai cho khách — tương tự cơ chế QuotePublicToken.
public class CloseTicketCommandHandler : IRequestHandler<CloseTicketCommand, TicketDto>
{
    private const string AuditTable = "TK_Ticket";
    private readonly ITicketRepository _ticketRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICsatRepository _csatRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CloseTicketCommandHandler> _logger;

    public CloseTicketCommandHandler(
        ITicketRepository ticketRepository,
        ICustomerRepository customerRepository,
        ICsatRepository csatRepository,
        IEmailService emailService,
        IConfiguration config,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher,
        ICurrentUserService currentUser,
        ILogger<CloseTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _customerRepository = customerRepository;
        _csatRepository = csatRepository;
        _emailService = emailService;
        _config = config;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TicketDto> Handle(CloseTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.Id);

        // Chặn Sale đóng Ticket không phải mình xử lý.
        if (_currentUser.Role == Roles.Sale && ticket.NhanVienXuLyId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền thao tác trên ticket của nhân viên khác.");

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

        // Tạo yêu cầu khảo sát hài lòng (CSAT) + gửi email công khai cho khách.
        // Lỗi gửi mail không được làm rollback việc đóng ticket.
        try
        {
            var customer = await _customerRepository.GetByIdAsync(ticket.KhachHangId, cancellationToken);
            if (customer?.Email is { Length: > 0 })
            {
                var token = await _csatRepository.CreateRequestAsync(ticket.Id, cancellationToken);
                var frontendBaseUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
                var csatLink = $"{frontendBaseUrl}/public/tickets/csat/{token}";

                await _emailService.GuiEmailKhaoSatHaiLongAsync(
                    ticket.KhachHangId, customer.TenKhachHang, customer.Email,
                    ticket.MaTicket, csatLink, cancellationToken);

                await _csatRepository.MarkEmailSentAsync(ticket.Id, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gửi email khảo sát hài lòng thất bại cho ticket {Id}", ticket.Id);
        }

        return newDto;
    }
}