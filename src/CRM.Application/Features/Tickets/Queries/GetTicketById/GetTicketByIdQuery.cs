using CRM.Application.Features.Tickets.DTOs;
using MediatR;

namespace CRM.Application.Features.Tickets.Queries.GetTicketById
{
    public record GetTicketByIdQuery(ulong Id) : IRequest<TicketDetailDto>;

    public class TicketDetailDto : TicketDto
    {
        public List<TicketPhanHoiDto> PhanHois { get; set; } = [];

        /// <summary>Kết quả khảo sát hài lòng (CSAT) của khách sau khi ticket đóng — null nếu ticket
        /// chưa từng đóng / chưa gửi khảo sát, hoặc DiemDanhGia null nếu khách chưa bấm đánh giá.</summary>
        public CsatDto? Csat { get; set; }
    }
}