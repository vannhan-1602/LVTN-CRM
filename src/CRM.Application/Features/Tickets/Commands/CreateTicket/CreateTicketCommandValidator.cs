using CRM.Domain.Enums;
using FluentValidation;

namespace CRM.Application.Features.Tickets.Commands.CreateTicket
{
    public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
    {
        public CreateTicketCommandValidator()
        {
            RuleFor(x => x.TieuDe)
                .NotEmpty().WithMessage("Tiêu đề không được để trống.")
                .MaximumLength(255).WithMessage("Tiêu đề không quá 255 ký tự.");

            RuleFor(x => x.KhachHangId)
                .GreaterThan(0UL).WithMessage("Khách hàng không hợp lệ.");

            RuleFor(x => x.MucDoUuTien)
                .Must(v => Enum.TryParse<TicketPriority>(v, out _))
                .WithMessage("Mức độ ưu tiên không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.MucDoUuTien));

            RuleFor(x => x.NguonTiepNhan)
                .Must(v => Enum.TryParse<TicketSource>(v, out _))
                .WithMessage("Nguồn tiếp nhận không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.NguonTiepNhan));

            RuleFor(x => x.FileDinhKem)
                .MaximumLength(500).WithMessage("Đường dẫn file không quá 500 ký tự.");
        }
    }
}