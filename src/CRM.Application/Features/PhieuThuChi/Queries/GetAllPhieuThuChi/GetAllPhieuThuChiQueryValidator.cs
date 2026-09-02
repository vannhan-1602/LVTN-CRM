using FluentValidation;

namespace CRM.Application.Features.PhieuThuChi.Queries.GetAllPhieuThuChi;

public class GetAllPhieuThuChiQueryValidator : AbstractValidator<GetAllPhieuThuChiQuery>
{
    public GetAllPhieuThuChiQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}