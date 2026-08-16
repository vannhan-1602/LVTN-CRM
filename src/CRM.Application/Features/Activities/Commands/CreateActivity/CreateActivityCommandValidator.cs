using FluentValidation;

namespace CRM.Application.Features.Activities.Commands.CreateActivity;

public class CreateActivityCommandValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityCommandValidator()
    {
        // Đồng bộ CreateOpportunityCommandValidator: dùng XOR (^), không phải OR (||) — Activity
        // chỉ được gắn với ĐÚNG MỘT trong hai (Khách hàng HOẶC Lead), không được cả hai cùng lúc.
        // Cột KhachHang_Id/Lead_Id trong DB đều nullable, không có CHECK constraint nào chặn việc
        // này ở tầng DB, nên phải chặn ở đây — nếu không, 1 Hoạt động có thể vừa gắn Khách hàng
        // vừa gắn Lead cùng lúc, không rõ nó thuộc về ai khi hiển thị/thống kê.
        RuleFor(x => x).Must(x => x.KhachHangId.HasValue ^ x.LeadId.HasValue)
            .WithMessage("Hoạt động phải gắn với đúng một Khách hàng hoặc một Lead, không được chọn cả hai hoặc bỏ trống cả hai.");

        RuleFor(x => x.LoaiHoatDong)
            .NotEmpty().WithMessage("Loại hoạt động không được để trống.")
            .MaximumLength(20);

        RuleFor(x => x.NoiDung).MaximumLength(255);
    }
}