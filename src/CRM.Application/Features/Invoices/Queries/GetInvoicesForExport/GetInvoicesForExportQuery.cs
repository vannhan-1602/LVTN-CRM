using CRM.Application.Common.Constants;
using CRM.Application.Features.Invoices.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Invoices;
using MediatR;

namespace CRM.Application.Features.Invoices.Queries.GetInvoicesForExport;

public record GetInvoicesForExportQuery(
    string? Search, string? TrangThaiThanhToan, ulong? KhachHangId
) : IRequest<List<InvoiceDto>>;

public class GetInvoicesForExportQueryHandler : IRequestHandler<GetInvoicesForExportQuery, List<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICurrentUserService _currentUser;

    public GetInvoicesForExportQueryHandler(IInvoiceRepository invoiceRepository, ICurrentUserService currentUser)
    {
        _invoiceRepository = invoiceRepository;
        _currentUser = currentUser;
    }

    public Task<List<InvoiceDto>> Handle(GetInvoicesForExportQuery request, CancellationToken ct)
    {
       
        uint? ownerUserId = _currentUser.Role == Roles.Sale ? _currentUser.UserId : null;

        return _invoiceRepository.GetForExportAsync(
            request.Search, request.TrangThaiThanhToan, request.KhachHangId, ownerUserId, ct);
    }
}