using CRM.Application.Features.Tickets.DTOs;
using MediatR;

namespace CRM.Application.Features.Tickets.Queries.GetTicketById
{
    public record GetTicketByIdQuery(ulong Id) : IRequest<TicketDetailDto>;

    public class TicketDetailDto : TicketDto
    {
        public List<TicketPhanHoiDto> PhanHois { get; set; } = [];
    }
}