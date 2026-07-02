using CRM.Application.Common.Exceptions;
using CRM.Application.Features.PhieuThuChi.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Invoices;
using CRM.Application.Interfaces.PhieuThuChi;
using CRM.Application.Interfaces.Common;
using CRM.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using DomainPhieuThuChi = CRM.Domain.Entities.Sales.PhieuThuChi;

namespace CRM.Application.Features.PhieuThuChi.Commands.CreatePhieuThuChi;

/// <summary>
/// Tạo một Phiếu Thu hoặc Phiếu Chi.
/// 
/// Nghiệp vụ chính:
/// - Loại 'Thu': ghi nhận tiền thực thu từ khách hàng cho 1 hóa đơn cụ thể.
///   Sau khi tạo phiếu thu, hệ thống tự động:
///   1. Cộng SoTien vào SoTienDaThu của hóa đơn liên kết.
///   2. Cập nhật TrangThaiThanhToan của hóa đơn (ChuaThanhToan → ThanhToan1Phan → HoanTat).
///   3. Kiểm tra không cho thu vượt quá TongTien của hóa đơn.
/// 
/// - Loại 'Chi': ghi nhận chi phí phát sinh, không bắt buộc gắn hóa đơn.
/// </summary>
public record CreatePhieuThuChiCommand(
    ulong? HoaDonId,
    ulong? KhachHangId,
    string LoaiPhieu,
    decimal SoTien
) : IRequest<PhieuThuChiDto>;

public class CreatePhieuThuChiCommandValidator : AbstractValidator<CreatePhieuThuChiCommand>
{
    public CreatePhieuThuChiCommandValidator()
    {
        RuleFor(x => x.LoaiPhieu)
            .Must(v => PaymentVoucherType.All.Contains(v))
            .WithMessage($"Loại phiếu không hợp lệ. Chỉ chấp nhận: {string.Join(", ", PaymentVoucherType.All)}.");

        RuleFor(x => x.SoTien)
            .GreaterThan(0m)
            .WithMessage("Số tiền phải lớn hơn 0.");

        // Phiếu Thu bắt buộc phải gắn với một hóa đơn cụ thể để kiểm soát thanh toán.
        When(x => x.LoaiPhieu == PaymentVoucherType.Thu, () =>
        {
            RuleFor(x => x.HoaDonId)
                .NotNull()
                .WithMessage("Phiếu Thu phải được gắn với một hóa đơn.")
                .GreaterThan(0UL)
                .WithMessage("Mã hóa đơn không hợp lệ.");
        });

        // Phiếu Chi phải có khách hàng hoặc hóa đơn để truy vết mục đích chi.
        When(x => x.LoaiPhieu == PaymentVoucherType.Chi, () =>
        {
            RuleFor(x => x)
                .Must(x => x.KhachHangId.HasValue || x.HoaDonId.HasValue)
                .WithMessage("Phiếu Chi phải gắn với khách hàng hoặc hóa đơn liên quan.");
        });
    }
}

public class CreatePhieuThuChiCommandHandler : IRequestHandler<CreatePhieuThuChiCommand, PhieuThuChiDto>
{
    private const string AuditTable = "KT_PhieuThuChi";

    private readonly IPhieuThuChiRepository _phieuThuChiRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<CreatePhieuThuChiCommandHandler> _logger;

    public CreatePhieuThuChiCommandHandler(
        IPhieuThuChiRepository phieuThuChiRepository,
        IInvoiceRepository invoiceRepository,
        ICustomerRepository customerRepository,
        ICurrentUserService currentUserService,
        IAuditLogPublisher auditLogPublisher,
        ILogger<CreatePhieuThuChiCommandHandler> logger)
    {
        _phieuThuChiRepository = phieuThuChiRepository;
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _currentUserService = currentUserService;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<PhieuThuChiDto> Handle(CreatePhieuThuChiCommand request, CancellationToken ct)
    {
        // ── 1. Validate hóa đơn và kiểm tra không thu vượt mức ────────────
        if (request.HoaDonId.HasValue)
        {
            var hoaDon = await _invoiceRepository.GetByIdAsync(request.HoaDonId.Value, ct)
                ?? throw new NotFoundException("Hóa đơn", request.HoaDonId.Value);

            if (request.LoaiPhieu == PaymentVoucherType.Thu)
            {
                if (hoaDon.TrangThaiThanhToan == InvoiceStatus.HoanTat)
                    throw new BusinessRuleException(
                        $"Hóa đơn {hoaDon.MaHoaDon} đã hoàn tất thanh toán, không thể tạo thêm phiếu thu.");

                var tongDaThu = await _phieuThuChiRepository.GetTongDaThuByHoaDonAsync(request.HoaDonId.Value, ct);
                var conLai = hoaDon.TongTien - tongDaThu;

                if (request.SoTien > conLai)
                    throw new BusinessRuleException(
                        $"Số tiền thu ({request.SoTien:N0} VNĐ) vượt quá số tiền còn lại của hóa đơn ({conLai:N0} VNĐ).");
            }
        }

        // ── 2. Validate khách hàng nếu có ─────────────────────────────────
        ulong? resolvedKhachHangId = request.KhachHangId;

        if (request.KhachHangId.HasValue)
        {
            _ = await _customerRepository.GetByIdAsync(request.KhachHangId.Value, ct)
                ?? throw new NotFoundException("Khách hàng", request.KhachHangId.Value);
        }
        else if (request.HoaDonId.HasValue)
        {
            // Nếu không truyền KhachHangId nhưng có HoaDonId, lấy KhachHangId từ hóa đơn
            var hoaDon = await _invoiceRepository.GetByIdAsync(request.HoaDonId.Value, ct);
            resolvedKhachHangId = hoaDon?.KhachHangId;
        }

        // ── 3. Tạo phiếu ──────────────────────────────────────────────────
        var maPhieu = await _phieuThuChiRepository.GenerateMaPhieuAsync(request.LoaiPhieu, ct);

        var phieu = new DomainPhieuThuChi
        {
            MaPhieu = maPhieu,
            LoaiPhieu = request.LoaiPhieu,
            KhachHangId = resolvedKhachHangId,
            HoaDonId = request.HoaDonId,
            SoTien = request.SoTien,
            NguoiLapId = _currentUserService.UserId.HasValue ? (uint?)_currentUserService.UserId.Value : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _phieuThuChiRepository.AddAsync(phieu, ct);

        // ── 4. Cập nhật SoTienDaThu + TrangThaiThanhToan của hóa đơn ──────
        // (chỉ áp dụng cho phiếu Thu gắn với hóa đơn)
        if (request.LoaiPhieu == PaymentVoucherType.Thu && request.HoaDonId.HasValue)
        {
            await _invoiceRepository.UpdateSoTienDaThuAsync(request.HoaDonId.Value, request.SoTien, ct);
        }

        // ── 5. Lấy DTO đầy đủ để trả về ──────────────────────────────────
        var dto = await _phieuThuChiRepository.GetByIdEnrichedAsync(created.Id, ct)
            ?? throw new BusinessRuleException("Tạo phiếu thu/chi thất bại.");

        // ── 6. Audit log ───────────────────────────────────────────────────
        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, created.Id, "INSERT",
                oldData: null, newData: dto, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for phieu {Id}", created.Id); }

        return dto;
    }
}
