using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Invoices.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Invoices;
using MediatR;

namespace CRM.Application.Features.Invoices.Queries.GetAllInvoices;

public record GetAllInvoicesQuery(
    int PageNumber, int PageSize, string? Search, string? TrangThaiThanhToan, ulong? KhachHangId
) : IRequest<PagedResult<InvoiceDto>>;

public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICurrentUserService _currentUser;

    public GetAllInvoicesQueryHandler(IInvoiceRepository invoiceRepository, ICurrentUserService currentUser)
    {
        _invoiceRepository = invoiceRepository;
        _currentUser = currentUser;
    }

    public Task<PagedResult<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken ct)
    {
        // Sale chỉ xem hóa đơn của khách hàng mình phụ trách. Manager/Accountant xem toàn bộ.
        uint? ownerUserId = _currentUser.Role == Roles.Sale ? _currentUser.UserId : null;

        return _invoiceRepository.GetPagedAsync(
            request.PageNumber, request.PageSize, request.Search, request.TrangThaiThanhToan,
            request.KhachHangId, ownerUserId, ct);
    }
}
