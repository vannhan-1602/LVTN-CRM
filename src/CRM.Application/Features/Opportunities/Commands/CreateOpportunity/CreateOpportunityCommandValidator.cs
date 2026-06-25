using FluentValidation;

namespace CRM.Application.Features.Opportunities.Commands.CreateOpportunity;

public class CreateOpportunityCommandValidator : AbstractValidator<CreateOpportunityCommand>
{
    public CreateOpportunityCommandValidator()
    {
        RuleFor(x => x.TenThuongVu).NotEmpty().MaximumLength(100);
        RuleFor(x => x).Must(x => x.KhachHangId.HasValue || x.LeadId.HasValue)
            .WithMessage("Cơ hội phải gắn với ít nhất một Khách hàng hoặc Lead.");
        RuleFor(x => x.TyLeThanhCong).InclusiveBetween(0, 100);
    }
}