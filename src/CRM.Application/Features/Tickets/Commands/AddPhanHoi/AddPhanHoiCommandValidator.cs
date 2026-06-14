using CRM.Domain.Enums;
using FluentValidation;

namespace CRM.Application.Features.Tickets.Commands.AddPhanHoi
{
    public class AddPhanHoiCommandValidator : AbstractValidator<AddPhanHoiCommand>
    {
        public AddPhanHoiCommandValidator()
        {
            RuleFor(x => x.TicketId).GreaterThan(0UL).WithMessage("TicketId không hợp lệ.");

            RuleFor(x => x.NoiDung)
                .NotEmpty().WithMessage("Nội dung phản hồi không được để trống.");

            RuleFor(x => x.LoaiPhanHoi)
                .NotEmpty().WithMessage("Loại phản hồi không được để trống.")
                .Must(v => Enum.TryParse<TicketPhanHoiLoai>(v, out _))
                .WithMessage("Loại phản hồi không hợp lệ.");

            RuleFor(x => x.FileDinhKem)
                .MaximumLength(500);
        }
    }
}