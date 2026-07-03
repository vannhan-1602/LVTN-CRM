using FluentValidation;

namespace CRM.Application.Features.Customers.Queries.GetAllCustomers;

public class GetAllCustomersQueryValidator : AbstractValidator<GetAllCustomersQuery>
{
    public GetAllCustomersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Số trang phải lớn hơn 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("Kích thước trang phải từ 1 đến 200.");

        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage("Từ khóa tìm kiếm không được vượt quá 100 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}