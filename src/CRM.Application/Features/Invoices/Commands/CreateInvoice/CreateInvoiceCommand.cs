using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Invoices.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Invoices;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Invoices.Commands.CreateInvoice;

/// <summary>
/// Tạo Hóa đơn thanh toán.
/// 
/// Hóa đơn có thể tạo theo 2 cách:
///   1. Gắn HopDongId: phát sinh từ hợp đồng đã ký (trường hợp phổ biến).
///      Hợp đồng phải đang ở trạng thái DangThucHien.
///      KhachHangId tự lấy từ hợp đồng, không cần truyền.
///   2. Không gắn HopDongId: bán lẻ không qua hợp đồng.
///      Bắt buộc truyền KhachHangId.
/// 
/// Sau khi tạo hóa đơn, kế toán tạo Phiếu Thu để ghi nhận tiền thực thu.
/// </summary>
public record CreateInvoiceCommand(
    ulong? HopDongId,
    ulong? KhachHangId,
    ulong? LichThanhToanId,
    decimal TongTien
) : IRequest<InvoiceDto>;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.TongTien)
            .GreaterThan(0m)
            .WithMessage("Tổng tiền hóa đơn phải lớn hơn 0.");

        // Nếu không có HopDongId thì bắt buộc phải có KhachHangId
        When(x => !x.HopDongId.HasValue, () =>
        {
            RuleFor(x => x.KhachHangId)
                .NotNull().WithMessage("Phải cung cấp khách hàng khi tạo hóa đơn không gắn hợp đồng.")
                .GreaterThan(0UL).WithMessage("Khách hàng không hợp lệ.");

            // Đợt trả góp chỉ có ý nghĩa khi gắn với hợp đồng
            RuleFor(x => x.LichThanhToanId)
                .Null().WithMessage("Không thể chọn đợt trả góp khi hóa đơn không gắn hợp đồng.");
        });
    }
}

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    private const string AuditTable = "KT_HoaDon";

    private readonly IInvoiceRepository _invoiceRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IContractRepository _contractRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogPublisher _auditLog;
    private readonly ILogger<CreateInvoiceCommandHandler> _logger;

    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepo,
        ICustomerRepository customerRepo,
        IContractRepository contractRepo,
        ICurrentUserService currentUser,
        IAuditLogPublisher auditLog,
        ILogger<CreateInvoiceCommandHandler> logger)
    {
        _invoiceRepo = invoiceRepo;
        _customerRepo = customerRepo;
        _contractRepo = contractRepo;
        _currentUser = currentUser;
        _auditLog = auditLog;
        _logger = logger;
    }

    public async Task<InvoiceDto> Handle(CreateInvoiceCommand request, CancellationToken ct)
    {
        ulong resolvedKhachHangId;

        // ── 1. Validate hợp đồng (nếu có) ───────────────────────────────
        if (request.HopDongId.HasValue)
        {
            // Khoá dòng hợp đồng TRƯỚC khi đọc bất kỳ số liệu tổng hợp nào (tổng đã xuất, đợt đã
            // dùng...). Toàn bộ command này đã chạy trong 1 transaction (TransactionBehavior), nên
            // nếu 2 request tạo hóa đơn cho CÙNG hợp đồng đến gần như đồng thời, request thứ 2 sẽ
            // phải chờ request thứ 1 commit xong mới được đọc tiếp — tránh cả 2 cùng đọc "còn hạn
            // mức" trước khi ai kịp ghi, dẫn tới vượt giá trị hợp đồng hoặc trùng đợt trả góp.
            await _contractRepo.LockHopDongForUpdateAsync(request.HopDongId.Value, ct);

            var contract = await _contractRepo.GetByIdEnrichedAsync(request.HopDongId.Value, ct)
                ?? throw new NotFoundException("Hợp đồng", request.HopDongId.Value);

            // Chỉ tạo hóa đơn từ hợp đồng đang còn hiệu lực
            if (contract.TrangThai != ContractStatus.DangThucHien)
                throw new BusinessRuleException(
                    $"Hợp đồng {contract.MaHopDong} đang ở trạng thái '{contract.TrangThai}', " +
                    "không thể tạo hóa đơn. Chỉ hợp đồng đang thực hiện mới được phép.");

            resolvedKhachHangId = contract.KhachHangId;

            if (request.LichThanhToanId.HasValue)
            {
                // ── Trả góp: hóa đơn phải ứng đúng 1 đợt của chính hợp đồng này ──
                var dot = await _contractRepo.GetLichThanhToanByIdAsync(request.LichThanhToanId.Value, ct)
                    ?? throw new NotFoundException("Đợt thanh toán", request.LichThanhToanId.Value);

                if (dot.HopDongId != request.HopDongId.Value)
                    throw new BusinessRuleException(
                        $"Đợt thanh toán #{request.LichThanhToanId.Value} không thuộc hợp đồng {contract.MaHopDong}.");

                if (dot.DaCoHoaDon)
                    throw new BusinessRuleException(
                        $"Đợt {dot.SoDot} của hợp đồng {contract.MaHopDong} đã có hóa đơn khác, không thể xuất trùng.");

                if (request.TongTien != dot.SoTien)
                    throw new BusinessRuleException(
                        $"Tổng tiền hóa đơn phải bằng đúng số tiền của đợt {dot.SoDot} ({dot.SoTien:N0} đ).");
            }
            else if (contract.GiaTri.HasValue)
            {
                // ── Thanh toán 1 lần: tổng đã xuất + hóa đơn mới không được vượt giá trị hợp đồng ──
                var tongDaXuat = await _invoiceRepo.GetTongDaXuatHoaDonByHopDongAsync(request.HopDongId.Value, ct);
                if (tongDaXuat + request.TongTien > contract.GiaTri.Value)
                    throw new BusinessRuleException(
                        $"Tổng tiền hóa đơn ({tongDaXuat + request.TongTien:N0} đ) vượt quá giá trị hợp đồng " +
                        $"{contract.MaHopDong} ({contract.GiaTri.Value:N0} đ). Đã xuất {tongDaXuat:N0} đ trước đó.");
            }
        }
        else
        {
            // ── 2. Validate khách hàng (trường hợp không có hợp đồng) ───
            _ = await _customerRepo.GetByIdAsync(request.KhachHangId!.Value, ct)
                ?? throw new NotFoundException("Khách hàng", request.KhachHangId.Value);

            resolvedKhachHangId = request.KhachHangId!.Value;
        }

        // ── 3. Kế toán/Manager mới được tạo hóa đơn ─────────────────────
        if (_currentUser.Role == Roles.Sale)
            throw new ForbiddenException("Chỉ kế toán hoặc quản lý mới được tạo hóa đơn.");

        // ── 4. Tạo hóa đơn ───────────────────────────────────────────────
        var maHoaDon = await _invoiceRepo.GenerateMaHoaDonAsync(ct);

        var invoice = new HoaDon
        {
            MaHoaDon = maHoaDon,
            HopDongId = request.HopDongId,
            LichThanhToanId = request.LichThanhToanId,
            KhachHangId = resolvedKhachHangId,
            TongTien = request.TongTien,
            SoTienDaThu = 0m,
            TrangThaiThanhToan = InvoiceStatus.ChuaThanhToan,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _invoiceRepo.AddAsync(invoice, ct);

        var dto = await _invoiceRepo.GetByIdEnrichedAsync(created.Id, ct)
            ?? throw new BusinessRuleException("Tạo hóa đơn thất bại.");

        // ── 5. Audit log ──────────────────────────────────────────────────
        try
        {
            await _auditLog.PublishAsync(AuditTable, created.Id, "INSERT",
                oldData: null, newData: dto, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audit log failed for invoice {Id}", created.Id);
        }

        return dto;
    }
}
