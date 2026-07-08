using CRM.Application.Interfaces.Customers;
using MediatR;

namespace CRM.Application.Features.Customers.Queries.CheckDuplicateCustomer;

public class CheckDuplicateCustomerQueryHandler
    : IRequestHandler<CheckDuplicateCustomerQuery, CheckDuplicateCustomerResult>
{
    private readonly ICustomerRepository _repo;
    public CheckDuplicateCustomerQueryHandler(ICustomerRepository repo) => _repo = repo;

    public async Task<CheckDuplicateCustomerResult> Handle(
        CheckDuplicateCustomerQuery request, CancellationToken cancellationToken)
    {
        var matches = await _repo.FindDuplicatesAsync(
            request.Email, request.SoDienThoai, request.MaSoThue, request.ExcludeId, cancellationToken);

        return new CheckDuplicateCustomerResult
        {
            IsDuplicate = matches.Count > 0,
            Matches = matches.Select(m => new DuplicateMatchDto
            {
                Id = m.Id,
                MaKhachHang = m.MaKhachHang,
                TenKhachHang = m.TenKhachHang,
                TrungTruong = m.TrungTruong
            }).ToList()
        };
    }
}
