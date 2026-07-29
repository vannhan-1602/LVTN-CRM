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
                .When(x => !string.IsNullOrWhiteSpace(x.TrangThai))
                // Không cho đóng ticket qua đường cập nhật thường — nhánh này không gửi khảo
                // sát hài lòng (CSAT) và cũng không set NgayDong. Đóng ticket bắt buộc phải
                // đi qua CloseTicketCommand (nút "Đóng ticket" riêng) để không bỏ sót 2 việc đó.
                .Must(v => !string.Equals(v, nameof(TicketStatus.Dong), StringComparison.OrdinalIgnoreCase))
                .WithMessage("Vui lòng dùng chức năng \"Đóng ticket\" riêng để đóng — không đóng qua cập nhật trạng thái.")
                .When(x => !string.IsNullOrWhiteSpace(x.TrangThai));
        }
    }
}