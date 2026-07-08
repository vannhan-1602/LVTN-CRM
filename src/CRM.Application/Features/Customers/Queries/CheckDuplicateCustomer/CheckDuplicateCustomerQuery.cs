using MediatR;

namespace CRM.Application.Features.Customers.Queries.CheckDuplicateCustomer;

public record CheckDuplicateCustomerQuery(
    string? Email, string? SoDienThoai, string? MaSoThue, ulong? ExcludeId)
    : IRequest<CheckDuplicateCustomerResult>;

public class CheckDuplicateCustomerResult
{
    public bool IsDuplicate { get; set; }
    public List<DuplicateMatchDto> Matches { get; set; } = new();
}

public class DuplicateMatchDto
{
    public ulong Id { get; set; }
    public string MaKhachHang { get; set; } = string.Empty;
    public string TenKhachHang { get; set; } = string.Empty;
    public string TrungTruong { get; set; } = string.Empty; // "Email" | "SoDienThoai" | "MaSoThue"
}
