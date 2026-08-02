using FluentValidation;

namespace CRM.Application.Features.Opportunities.Commands.CreateOpportunity;

public class CreateOpportunityCommandValidator : AbstractValidator<CreateOpportunityCommand>
{
    public CreateOpportunityCommandValidator()
    {
        RuleFor(x => x.TenThuongVu).NotEmpty().MaximumLength(100);
        RuleFor(x => x)
            .Must(x => x.KhachHangId.HasValue ^ x.LeadId.HasValue)
            .WithMessage("Cơ hội phải gắn với đúng một Khách hàng hoặc một Lead, không được chọn cả hai hoặc bỏ trống cả hai.");
        RuleFor(x => x.TyLeThanhCong).InclusiveBetween(0, 100);
    }
}