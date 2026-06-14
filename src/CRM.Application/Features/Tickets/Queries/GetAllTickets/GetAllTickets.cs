using CRM.Domain.Enums;
using FluentValidation;

namespace CRM.Application.Features.Tickets.Queries.GetAllTickets
{
    public class GetAllTicketsQueryValidator : AbstractValidator<GetAllTicketsQuery>
    {
        public GetAllTicketsQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

            RuleFor(x => x.TrangThai)
                .Must(v => Enum.TryParse<TicketStatus>(v, out _))
                .WithMessage("Trạng thái không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.TrangThai));

            RuleFor(x => x.MucDoUuTien)
                .Must(v => Enum.TryParse<TicketPriority>(v, out _))
                .WithMessage("Mức độ ưu tiên không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.MucDoUuTien));
        }
    }
}