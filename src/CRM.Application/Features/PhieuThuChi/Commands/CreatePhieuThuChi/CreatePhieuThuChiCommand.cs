using CRM.Application.Common.Exceptions;
using CRM.Application.Features.PhieuThuChi.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Invoices;
using CRM.Application.Interfaces.PhieuThuChi;
using CRM.Application.Services;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using DomainPhieuThuChi = CRM.Domain.Entities.Sales.PhieuThuChi;

namespace CRM.Application.Features.PhieuThuChi.Commands.CreatePhieuThuChi;

/// <summary>
/// Tạo Phiếu Thu hoặc Phiếu Chi.
///
/// Phiếu Thu:
///   - Bắt buộc gắn HoaDonId.
///   - Hệ thống kiểm tra hóa đơn chưa hoàn tất và số tiền không vượt mức còn lại.
///   - Sau khi tạo, tự cập nhật SoTienDaThu + TrangThaiThanhToan hóa đơn (atomic).
///
/// Phiếu Chi:
///   - Phải có KhachHangId hoặc HoaDonId để truy vết mục đích chi.
///   - Không ảnh hưởng trạng thái hóa đơn.
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

        When(x => x.LoaiPhieu == PaymentVoucherType.Thu, () =>
        {
            RuleFor(x => x.HoaDonId)
                .NotNull().WithMessage("Phiếu Thu phải gắn với một hóa đơn.")
                .GreaterThan(0UL).WithMessage("Mã hóa đơn không hợp lệ.");
        });

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

    private readonly IPhieuThuChiRepository _phieuThuChiRepo;
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IContractRepository _contractRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLog;
    private readonly LoyaltyService _loyaltyService;
    private readonly ILogger<CreatePhieuThuChiCommandHandler> _logger;

    public CreatePhieuThuChiCommandHandler(
        IPhieuThuChiRepository phieuThuChiRepo,
        IInvoiceRepository invoiceRepo,
        IContractRepository contractRepo,
        ICustomerRepository customerRepo,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLog,
        LoyaltyService loyaltyService,
        ILogger<CreatePhieuThuChiCommandHandler> logger)
    {
        _phieuThuChiRepo = phieuThuChiRepo;
        _invoiceRepo = invoiceRepo;
        _contractRepo = contractRepo;
        _customerRepo = customerRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
        _loyaltyService = loyaltyService;
        _logger = logger;
    }

    public async Task<PhieuThuChiDto> Handle(CreatePhieuThuChiCommand request, CancellationToken ct)
    {
        // ── 1. Validate hóa đơn ───────────────────────────────────────────
        ulong? resolvedKhachHangId = request.KhachHangId;
        CRM.Domain.Entities.Sales.HoaDon? hoaDon = null;

        if (request.HoaDonId.HasValue)
        {
            hoaDon = await _invoiceRepo.GetByIdAsync(request.HoaDonId.Value, ct)
                ?? throw new NotFoundException("Hóa đơn", request.HoaDonId.Value);

            // Tự lấy KhachHangId từ hóa đơn nếu không truyền
            resolvedKhachHangId ??= hoaDon.KhachHangId;

            if (request.LoaiPhieu == PaymentVoucherType.Thu)
            {
                if (hoaDon.TrangThaiThanhToan == InvoiceStatus.HoanTat)
                    throw new BusinessRuleException(
                        $"Hóa đơn {hoaDon.MaHoaDon} đã hoàn tất thanh toán.");

                // Tính số tiền còn lại dựa trên tổng phiếu thu thực tế trong DB
                // (không dùng SoTienDaThu trên entity vì có thể có race condition đọc)
                var tongDaThu = await _phieuThuChiRepo.GetTongDaThuByHoaDonAsync(request.HoaDonId.Value, ct);
                var conLai = hoaDon.TongTien - tongDaThu;

                if (request.SoTien > conLai)
                    throw new BusinessRuleException(
                        $"Số tiền thu ({request.SoTien:N0} đ) vượt quá số tiền còn lại ({conLai:N0} đ).");
            }
        }

        // ── 2. Validate khách hàng tồn tại ───────────────────────────────
        if (resolvedKhachHangId.HasValue)
        {
            _ = await _customerRepo.GetByIdAsync(resolvedKhachHangId.Value, ct)
                ?? throw new NotFoundException("Khách hàng", resolvedKhachHangId.Value);
        }

        // ── 3+4. Tạo phiếu (+ cập nhật SoTienDaThu hóa đơn nếu là Phiếu Thu) ─────
        // Phiếu Thu: bọc chung 1 transaction — nếu UpdateSoTienDaThuAsync bị chặn atomic (2
        // phiếu thu tạo đồng thời cùng qua được bước validate "còn lại" ở bước 1), Phiếu Thu
        // vừa tạo PHẢI rollback theo, không được để lại phiếu "ma" không khớp với hóa đơn.
        // Phiếu Chi: không đụng tới hóa đơn/trạng thái thanh toán nào cần giữ nhất quán, nên
        // không cần bọc transaction — chi tiền tiếp khách không có rủi ro race condition này.
        DomainPhieuThuChi created;
        decimal? soTienDaThuSauKhiCong = null;
        decimal? tongTienHoaDon = null;

        if (request.LoaiPhieu == PaymentVoucherType.Thu && request.HoaDonId.HasValue)
        {
            (created, soTienDaThuSauKhiCong, tongTienHoaDon) = await _unitOfWork.ExecuteInTransactionAsync(
                async () =>
                {
                    var maPhieuThu = await _phieuThuChiRepo.GenerateMaPhieuAsync(request.LoaiPhieu, ct);
                    var phieuThu = new DomainPhieuThuChi
                    {
                        MaPhieu = maPhieuThu,
                        LoaiPhieu = request.LoaiPhieu,
                        KhachHangId = resolvedKhachHangId,
                        HoaDonId = request.HoaDonId,
                        SoTien = request.SoTien,
                        NguoiLapId = _currentUser.UserId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    var createdPhieu = await _phieuThuChiRepo.AddAsync(phieuThu, ct);

                    var (thanhCong, daThu, tongTien) =
                        await _invoiceRepo.UpdateSoTienDaThuAsync(request.HoaDonId.Value, request.SoTien, ct);

                    if (!thanhCong)
                        throw new BusinessRuleException(
                            "Có phiếu thu khác vừa được tạo cho hóa đơn này cùng lúc, khiến tổng tiền thu vượt quá hóa đơn. Vui lòng thử lại.");

                    return (createdPhieu, daThu, tongTien);
                }, ct);

            // Hóa đơn vừa hoàn tất VÀ ứng với 1 đợt trả góp → đồng bộ trạng thái đợt đó,
            // tránh job nhắc thanh toán báo "quá hạn" oan cho đợt đã thu xong. Việc này không
            // cần chung transaction với trên — lỗi ở đây chỉ log cảnh báo, không hủy giao dịch
            // thu tiền đã ghi nhận thành công.
            if (soTienDaThuSauKhiCong >= tongTienHoaDon && hoaDon?.LichThanhToanId.HasValue == true)
            {
                try
                {
                    await _contractRepo.MarkLichThanhToanDaThanhToanAsync(hoaDon.LichThanhToanId.Value, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Không đồng bộ được trạng thái đợt trả góp {LichThanhToanId} sau khi hóa đơn {HoaDonId} hoàn tất.",
                        hoaDon.LichThanhToanId.Value, request.HoaDonId.Value);
                }
            }
        }
        else
        {
            var maPhieu = await _phieuThuChiRepo.GenerateMaPhieuAsync(request.LoaiPhieu, ct);
            var phieu = new DomainPhieuThuChi
            {
                MaPhieu = maPhieu,
                LoaiPhieu = request.LoaiPhieu,
                KhachHangId = resolvedKhachHangId,
                HoaDonId = request.HoaDonId,
                SoTien = request.SoTien,
                NguoiLapId = _currentUser.UserId,   // uint? trực tiếp, không cần cast
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            created = await _phieuThuChiRepo.AddAsync(phieu, ct);
        }

        // ── 5. Xử lý Loyalty (tích điểm + tính hạng + voucher + email) ──────
        // QUAN TRỌNG: await trực tiếp trong cùng request scope (KHÔNG dùng Task.Run) —
        // _loyaltyService phụ thuộc CrmDbContext (Scoped theo request); nếu chạy nền bằng
        // Task.Run, request có thể trả response và Scope (kèm DbContext) bị dispose trước khi
        // task nền chạy xong, gây ObjectDisposedException âm thầm khiến điểm/voucher/email
        // không được xử lý mà không ai biết. Bọc try/catch để lỗi Loyalty không rollback
        // giao dịch thu tiền chính (đã ghi nhận ở bước 3-4).
        if (request.LoaiPhieu == PaymentVoucherType.Thu && request.HoaDonId.HasValue)
        {
            var hoaDonForLoyalty = await _invoiceRepo.GetByIdAsync(request.HoaDonId.Value, ct);
            var customerForEmail = resolvedKhachHangId.HasValue
                ? await _customerRepo.GetByIdAsync(resolvedKhachHangId.Value, ct)
                : null;

            if (hoaDonForLoyalty is not null)
            {
                try
                {
                    await _loyaltyService.XuLySauPhieuThuAsync(
                        khachHangId: hoaDonForLoyalty.KhachHangId,
                        tenKhachHang: customerForEmail?.TenKhachHang ?? "Quý khách",
                        khachHangEmail: customerForEmail?.Email,
                        maHoaDon: hoaDonForLoyalty.MaHoaDon,
                        soTienThu: request.SoTien,
                        hoaDonId: hoaDonForLoyalty.Id,
                        phieuThuChiId: created.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Loyalty] Lỗi xử lý sau phiếu thu KH {Id}", hoaDonForLoyalty.KhachHangId);
                }
            }
        }

        // ── 6. Trả về DTO đầy đủ ─────────────────────────────────────────
        var dto = await _phieuThuChiRepo.GetByIdEnrichedAsync(created.Id, ct)
            ?? throw new BusinessRuleException("Tạo phiếu thu/chi thất bại.");

        // ── 7. Audit log ──────────────────────────────────────────────────
        try
        {
            await _auditLog.PublishAsync(AuditTable, created.Id, "INSERT",
                oldData: null, newData: dto, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audit log failed for PhieuThuChi {Id}", created.Id);
        }

        return dto;
    }
}