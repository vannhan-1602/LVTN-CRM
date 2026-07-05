using CRM.Application.Common.Models;
using CRM.Application.Features.Customers.DTOs;
using MediatR;

namespace CRM.Application.Features.Customers.Queries.GetAllCustomers;

public record GetAllCustomersQuery(
    int PageNumber,
    int PageSize,
    string? Search,
    ushort? LoaiKhachHangId,    
    ushort? TinhTrangId,
    bool? IsDeleted = null
) : IRequest<PagedResult<CustomerDto>>;