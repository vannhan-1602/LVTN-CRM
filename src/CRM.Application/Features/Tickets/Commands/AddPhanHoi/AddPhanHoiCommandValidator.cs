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
                .WithMessage("Loại phản hồi không hợp lệ.")
                // Đóng ticket qua đường phản hồi này sẽ bỏ qua bước gửi khảo sát hài lòng
                // (CSAT) — bước đó chỉ được kích hoạt đúng trong CloseTicketCommandHandler.
                // Bắt buộc đi qua API đóng ticket riêng để không bỏ sót CSAT.
                .Must(v => v != nameof(TicketPhanHoiLoai.DongTicket))
                .WithMessage("Vui lòng dùng chức năng \"Đóng ticket\" riêng để đóng — không đóng qua phản hồi.");

            RuleFor(x => x.FileDinhKem)
                .MaximumLength(500);
        }
    }
}