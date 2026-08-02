using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Tickets.Mappings;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Tickets;
using CRM.Domain.Entities.Tickets;
using CRM.Domain.Enums;
using MediatR;

namespace CRM.Application.Features.Tickets.Queries.GetTicketById;

public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, TicketDetailDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICsatRepository _csatRepository;
    private readonly ICurrentUserService _currentUser;

    public GetTicketByIdQueryHandler(ITicketRepository ticketRepository,
        ICustomerRepository customerRepository,
        ICsatRepository csatRepository, ICurrentUserService currentUser)
    {
        _ticketRepository = ticketRepository;
        _customerRepository = customerRepository;
        _csatRepository = csatRepository;
        _currentUser = currentUser;
    }

    public async Task<TicketDetailDto> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.Id);

        // Sale được xem ticket của chính mình hoặc ticket chưa ai nhận (để có thể tự nhận xử lý) —
        // khớp với quy tắc quyền hạn ở AssignTicketCommandHandler. Không được xem ticket của
        // đồng nghiệp khác.
        if (_currentUser.Role == Roles.Sale &&
            ticket.NhanVienXuLyId != _currentUser.UserId &&
            ticket.NhanVienXuLyId != null)
            throw new ForbiddenException("Bạn không có quyền xem dữ liệu của nhân viên khác.");

        var phanHois = await _ticketRepository.GetPhanHoisAsync(request.Id, cancellationToken);

        // Yêu cầu CSAT chỉ được tạo khi đóng ticket (CloseTicketCommandHandler) -> chỉ cần
        // tra khi ticket đã đóng, tránh 1 query thừa cho ticket còn đang xử lý.
        var csat = ticket.TrangThai == TicketStatus.Dong
            ? await _csatRepository.GetByTicketIdAsync(request.Id, cancellationToken)
            : null;

        // Nhân viên xử lý cần biết khách hàng là ai để liên hệ (tên/SĐT/email) — ticket chỉ lưu
        // KhachHangId nên phải tra thêm; nếu khách hàng đã bị xóa mềm thì bỏ qua, không chặn
        // việc xem ticket.
        var khachHang = await _customerRepository.GetByIdEnrichedAsync(ticket.KhachHangId, cancellationToken);

        var dto = TicketMapper.ToDto(ticket);
        return new TicketDetailDto
        {
            Id = dto.Id,
            MaTicket = dto.MaTicket,
            TieuDe = dto.TieuDe,
            MoTa = dto.MoTa,
            FileDinhKem = dto.FileDinhKem,
            LoaiTicketId = dto.LoaiTicketId,
            KhachHangId = dto.KhachHangId,
            TenKhachHang = khachHang?.TenKhachHang,
            EmailKhachHang = khachHang?.Email,
            SoDienThoaiKhachHang = khachHang?.SoDienThoai,
            HopDongId = dto.HopDongId,
            SanPhamId = dto.SanPhamId,
            MucDoUuTien = dto.MucDoUuTien,
            NguonTiepNhan = dto.NguonTiepNhan,
            TrangThai = dto.TrangThai,
            NhanVienTiepNhanId = dto.NhanVienTiepNhanId,
            NhanVienXuLyId = dto.NhanVienXuLyId,
            NgayHenXuLy = dto.NgayHenXuLy,
            NgayDong = dto.NgayDong,
            LyDoDong = dto.LyDoDong,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            Csat = csat,
            PhanHois = phanHois
                .OrderByDescending(p => p.CreatedAt)
                .Select(TicketMapper.ToDto)
                .ToList()
        };
    }
}