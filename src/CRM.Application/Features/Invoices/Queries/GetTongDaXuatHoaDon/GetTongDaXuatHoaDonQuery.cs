using CRM.Application.Interfaces.Invoices;
using MediatR;

namespace CRM.Application.Features.Invoices.Queries.GetTongDaXuatHoaDon;

/// <summary>
/// Tổng tiền đã xuất hóa đơn cho 1 hợp đồng — dùng ở form tạo hóa đơn (Frontend
/// CreateInvoiceModal) để tự tính số tiền còn lại khi hợp đồng thanh toán 1 lần.
/// </summary>
public record GetTongDaXuatHoaDonQuery(ulong HopDongId) : IRequest<decimal>;

public class GetTongDaXuatHoaDonQueryHandler : IRequestHandler<GetTongDaXuatHoaDonQuery, decimal>
{
    private readonly IInvoiceRepository _invoiceRepo;
    public GetTongDaXuatHoaDonQueryHandler(IInvoiceRepository invoiceRepo) => _invoiceRepo = invoiceRepo;

    public Task<decimal> Handle(GetTongDaXuatHoaDonQuery request, CancellationToken ct) =>
        _invoiceRepo.GetTongDaXuatHoaDonByHopDongAsync(request.HopDongId, ct);
}
