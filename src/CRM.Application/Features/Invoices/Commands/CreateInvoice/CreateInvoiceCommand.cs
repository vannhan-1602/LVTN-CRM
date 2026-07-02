using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Invoices.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Invoices;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Invoices.Commands.CreateInvoice;

// Tạo Hóa đơn cho 1 khách hàng, có thể gắn kèm Hợp đồng gốc (nếu phát sinh từ
// hợp đồng đã ký) hoặc không (hóa đơn bán lẻ không qua hợp đồng).

public record CreateInvoiceCommand(
    ulong? HopDongId, ulong KhachHangId, decimal TongTien
) : IRequest<InvoiceDto>;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.KhachHangId).GreaterThan(0UL).WithMessage("Khách hàng không hợp lệ.");
        RuleFor(x => x.TongTien).GreaterThan(0m).WithMessage("Tổng tiền hóa đơn phải lớn hơn 0.");
    }
}

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    private const string AuditTable = "KT_HoaDon";
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<CreateInvoiceCommandHandler> _logger;

    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository, ICustomerRepository customerRepository,
        IContractRepository contractRepository, IAuditLogPublisher auditLogPublisher,
        ILogger<CreateInvoiceCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _contractRepository = contractRepository;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<InvoiceDto> Handle(CreateInvoiceCommand request, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(request.KhachHangId, ct)
            ?? throw new NotFoundException("Khách hàng", request.KhachHangId);

        if (request.HopDongId.HasValue)
        {
            var contract = await _contractRepository.GetByIdAsync(request.HopDongId.Value, ct)
                ?? throw new NotFoundException("Hợp đồng", request.HopDongId.Value);

            if (contract.KhachHangId != request.KhachHangId)
                throw new BusinessRuleException("Hợp đồng này không thuộc về khách hàng đã chọn.");
        }

        var maHoaDon = await _invoiceRepository.GenerateMaHoaDonAsync(ct);

        var invoice = new HoaDon
        {
            MaHoaDon = maHoaDon,
            HopDongId = request.HopDongId,
            KhachHangId = request.KhachHangId,
            TongTien = request.TongTien,
            SoTienDaThu = 0m,
            TrangThaiThanhToan = InvoiceStatus.ChuaThanhToan,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _invoiceRepository.AddAsync(invoice, ct);

        var dto = await _invoiceRepository.GetByIdEnrichedAsync(created.Id, ct)
            ?? throw new BusinessRuleException("Tạo hóa đơn thất bại.");

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, created.Id, "INSERT",
                oldData: null, newData: dto, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for invoice {Id}", created.Id); }

        return dto;
    }
}
