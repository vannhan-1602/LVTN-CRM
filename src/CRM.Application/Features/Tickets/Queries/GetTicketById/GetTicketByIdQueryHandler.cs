using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Tickets.Mappings;
using CRM.Application.Interfaces.Tickets;
using CRM.Domain.Entities.Tickets;
using MediatR;

namespace CRM.Application.Features.Tickets.Queries.GetTicketById
{
    public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, TicketDetailDto>
    {
        private readonly ITicketRepository _ticketRepository;
        public GetTicketByIdQueryHandler(ITicketRepository ticketRepository)
            => _ticketRepository = ticketRepository;

        public async Task<TicketDetailDto> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Ticket), request.Id);

            var phanHois = await _ticketRepository.GetPhanHoisAsync(request.Id, cancellationToken);

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
                PhanHois = phanHois
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(TicketMapper.ToDto)
                    .ToList()
            };
        }
    }
}