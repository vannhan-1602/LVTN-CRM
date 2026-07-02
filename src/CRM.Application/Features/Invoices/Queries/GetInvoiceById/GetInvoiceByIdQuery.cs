using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Invoices.DTOs;
using CRM.Application.Interfaces.Invoices;
using CRM.Domain.Entities.Sales;
using MediatR;

namespace CRM.Application.Features.Invoices.Queries.GetInvoiceById;

public record GetInvoiceByIdQuery(ulong Id) : IRequest<InvoiceDto>;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    public GetInvoiceByIdQueryHandler(IInvoiceRepository invoiceRepository) => _invoiceRepository = invoiceRepository;

    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken ct) =>
        await _invoiceRepository.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(HoaDon), request.Id);
}
