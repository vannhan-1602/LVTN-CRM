using FluentValidation;

namespace CRM.Application.Features.Opportunities.Commands.UpdateOpportunity;

public class UpdateOpportunityCommandValidator : AbstractValidator<UpdateOpportunityCommand>
{
    public UpdateOpportunityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
        RuleFor(x => x.TenThuongVu).NotEmpty().MaximumLength(100);
        RuleFor(x => x)
            .Must(x => x.KhachHangId.HasValue ^ x.LeadId.HasValue)
            .WithMessage("Cơ hội phải gắn với đúng một Khách hàng hoặc một Lead, không được chọn cả hai hoặc bỏ trống cả hai.");
        RuleFor(x => x.TyLeThanhCong).InclusiveBetween(0, 100);
    }
}