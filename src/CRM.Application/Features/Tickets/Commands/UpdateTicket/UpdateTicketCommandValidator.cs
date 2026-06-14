using CRM.Domain.Enums;
using FluentValidation;

namespace CRM.Application.Features.Tickets.Commands.UpdateTicket
{
    public class UpdateTicketCommandValidator : AbstractValidator<UpdateTicketCommand>
    {
        public UpdateTicketCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0UL).WithMessage("Id không hợp lệ.");

            RuleFor(x => x.TieuDe)
                .NotEmpty().WithMessage("Tiêu đề không được để trống.")
                .MaximumLength(255);

            RuleFor(x => x.MucDoUuTien)
                .Must(v => Enum.TryParse<TicketPriority>(v, out _))
                .WithMessage("Mức độ ưu tiên không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.MucDoUuTien));

            RuleFor(x => x.NguonTiepNhan)
                .Must(v => Enum.TryParse<TicketSource>(v, out _))
                .WithMessage("Nguồn tiếp nhận không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.NguonTiepNhan));

            RuleFor(x => x.TrangThai)
                .Must(v => Enum.TryParse<TicketStatus>(v, out _))
                .WithMessage("Trạng thái không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.TrangThai));
        }
    }
}