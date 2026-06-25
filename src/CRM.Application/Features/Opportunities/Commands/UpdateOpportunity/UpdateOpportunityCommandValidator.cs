using FluentValidation;

namespace CRM.Application.Features.Opportunities.Commands.UpdateOpportunity;

public class UpdateOpportunityCommandValidator : AbstractValidator<UpdateOpportunityCommand>
{
    public UpdateOpportunityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
        RuleFor(x => x.TenThuongVu).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TyLeThanhCong).InclusiveBetween(0, 100);
    }
}