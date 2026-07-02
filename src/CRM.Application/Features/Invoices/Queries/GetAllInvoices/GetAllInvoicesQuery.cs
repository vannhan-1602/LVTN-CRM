using CRM.Application.Common.Models;
using CRM.Application.Features.Invoices.DTOs;
using CRM.Application.Interfaces.Invoices;
using MediatR;

namespace CRM.Application.Features.Invoices.Queries.GetAllInvoices;

public record GetAllInvoicesQuery(
    int PageNumber, int PageSize, string? Search, string? TrangThaiThanhToan, ulong? KhachHangId
) : IRequest<PagedResult<InvoiceDto>>;

public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    public GetAllInvoicesQueryHandler(IInvoiceRepository invoiceRepository) => _invoiceRepository = invoiceRepository;

    public Task<PagedResult<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken ct) =>
        _invoiceRepository.GetPagedAsync(
            request.PageNumber, request.PageSize, request.Search, request.TrangThaiThanhToan,
            request.KhachHangId, ct);
}
