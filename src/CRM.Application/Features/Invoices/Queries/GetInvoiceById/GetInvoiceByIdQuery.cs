using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Invoices.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Invoices;
using CRM.Domain.Entities.Sales;
using MediatR;

namespace CRM.Application.Features.Invoices.Queries.GetInvoiceById;

public record GetInvoiceByIdQuery(ulong Id) : IRequest<InvoiceDto>;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICurrentUserService _currentUser;

    public GetInvoiceByIdQueryHandler(IInvoiceRepository invoiceRepository, ICurrentUserService currentUser)
    {
        _invoiceRepository = invoiceRepository;
        _currentUser = currentUser;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken ct)
    {
        var invoice = await _invoiceRepository.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(HoaDon), request.Id);

        // Chặn Sale xem hóa đơn của khách hàng không phải mình phụ trách.
        if (_currentUser.Role == Roles.Sale && invoice.NhanVienPhuTrachId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền xem hóa đơn của khách hàng khác.");

        return invoice;
    }
}
