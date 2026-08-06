using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Email;
using CRM.Application.Interfaces.Loyalty;
using CRM.Application.Interfaces.Quotes;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Contracts.Commands.SendContractEmail;

// Gửi hợp đồng (kèm file PDF) qua email cho khách hàng của hợp đồng.
//
// Nguyên tắc dữ liệu: số liệu có giá trị pháp lý/tiền bạc (mã HĐ, giá trị, sản phẩm, lịch trả
// góp, thông tin khách hàng) LUÔN đọc lại từ DB — KHÔNG tin theo dữ liệu request gửi lên, để
// tránh trường hợp ai đó sửa số tiền trên trình duyệt trước khi gửi. Chỉ phần "trình bày" (bên A,
// bổ sung bên B, nội dung từng điều khoản) lấy từ request, vì các trường này vốn không được lưu
// ở đâu trong DB theo thiết kế của ContractPrintPage.
//
// Ràng buộc nghiệp vụ/pháp lý bắt buộc trước khi cho gửi (xem chi tiết trong Validator/Handler):
//  1. Không được gửi hợp đồng thiếu thông tin bên A/bên B (tên, địa chỉ, MST, người đại diện...) —
//     một hợp đồng thiếu thông tin nhận dạng các bên có thể bị coi là không xác định được chủ thể,
//     rủi ro vô hiệu/khó thực thi theo Bộ luật Dân sự.
//  2. Không gửi khi hợp đồng đã ở trạng thái Thanh lý (không còn ý nghĩa yêu cầu khách ký nữa).
//  3. Hợp đồng phải đã có giá trị (GiaTri hoặc tổng tiền sản phẩm) > 0.
//  4. Chặn gửi trùng liên tiếp trong thời gian ngắn (chống bấm nhầm/spam khách).
//  5. Ghi log ai (nhân viên nào) đã gửi — phục vụ truy vết trách nhiệm dù không có cột DB riêng.
public record SendContractEmailCommand(ulong HopDongId, SendContractEmailRequestDto Request)
    : IRequest<SendContractEmailResultDto>;

public class SendContractEmailCommandValidator : AbstractValidator<SendContractEmailCommand>
{
    public SendContractEmailCommandValidator()
    {
        RuleFor(x => x.HopDongId).GreaterThan(0UL);
        RuleFor(x => x.Request.BaoHanhThang).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.MucPhatViPham).InclusiveBetween(0, 8)
            .WithMessage("Mức phạt vi phạm không được vượt quá 8% theo Điều 301 Luật Thương mại 2005.");
        RuleFor(x => x.Request.SoBan).GreaterThanOrEqualTo(2);
        RuleFor(x => x.Request.LoiNhan).MaximumLength(1000);

        RuleFor(x => x.Request.DiaDiemKy)
            .NotEmpty().WithMessage("Thiếu địa điểm ký hợp đồng.");

        // Bên A (bên cung cấp) — bắt buộc đủ thông tin nhận dạng pháp nhân + người đại diện ký.
        // Đây là dữ liệu KHÔNG có trong DB (không lưu theo thiết kế), nên phải chặn ở đây —
        // nếu thiếu, PDF gửi khách sẽ có chỗ trống "……", không chấp nhận được cho hợp đồng chính thức.
        RuleFor(x => x.Request.BenA.TenCongTy).NotEmpty().WithMessage("Thiếu tên công ty Bên A.");
        RuleFor(x => x.Request.BenA.DiaChi).NotEmpty().WithMessage("Thiếu địa chỉ Bên A.");
        RuleFor(x => x.Request.BenA.MaSoThue).NotEmpty().WithMessage("Thiếu mã số thuế Bên A.");
        RuleFor(x => x.Request.BenA.NguoiDaiDien).NotEmpty().WithMessage("Thiếu người đại diện Bên A.");
        RuleFor(x => x.Request.BenA.ChucVu).NotEmpty().WithMessage("Thiếu chức vụ người đại diện Bên A.");

        // Bên B: tên/MST/điện thoại/email đã lấy tự động từ hồ sơ khách hàng (không thuộc request
        // này) — chỉ còn thiếu địa chỉ + người đại diện ký kết, cũng bắt buộc phải điền.
        RuleFor(x => x.Request.BenB.DiaChi).NotEmpty().WithMessage("Thiếu địa chỉ Bên B.");
        RuleFor(x => x.Request.BenB.NguoiDaiDien).NotEmpty().WithMessage("Thiếu người đại diện Bên B.");
        RuleFor(x => x.Request.BenB.ChucVu).NotEmpty().WithMessage("Thiếu chức vụ người đại diện Bên B.");
    }
}

public class SendContractEmailCommandHandler : IRequestHandler<SendContractEmailCommand, SendContractEmailResultDto>
{
    private const string HINH_THUC_THANH_TOAN_MOT_LAN = "ThanhToanMotLan";

    /// <summary>Chặn gửi trùng nếu lần gửi thành công gần nhất cách đây chưa đủ khoảng thời gian
    /// này — tránh nhân viên bấm nhầm 2 lần khiến khách nhận 2 email liên tiếp.</summary>
    private static readonly TimeSpan KhoangCachToiThieu = TimeSpan.FromMinutes(2);

    private readonly IContractRepository _contractRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IQuoteRepository _quoteRepository;
    private readonly ILoyaltyRepository _loyaltyRepository;
    private readonly IContractPdfGenerator _pdfGenerator;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SendContractEmailCommandHandler> _logger;

    public SendContractEmailCommandHandler(
        IContractRepository contractRepository, ICustomerRepository customerRepository,
        IQuoteRepository quoteRepository, ILoyaltyRepository loyaltyRepository,
        IContractPdfGenerator pdfGenerator, IEmailService emailService,
        ICurrentUserService currentUser, ILogger<SendContractEmailCommandHandler> logger)
    {
        _contractRepository = contractRepository;
        _customerRepository = customerRepository;
        _quoteRepository = quoteRepository;
        _loyaltyRepository = loyaltyRepository;
        _pdfGenerator = pdfGenerator;
        _emailService = emailService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<SendContractEmailResultDto> Handle(SendContractEmailCommand cmd, CancellationToken ct)
    {
        var req = cmd.Request;

        var contract = await _contractRepository.GetByIdEnrichedAsync(cmd.HopDongId, ct)
            ?? throw new NotFoundException(nameof(HopDong), cmd.HopDongId);

        // Hợp đồng đã thanh lý xong thì không còn ý nghĩa gửi để yêu cầu khách ký nữa.
        if (contract.TrangThai == ContractStatus.ThanhLy)
            throw new BusinessRuleException("Hợp đồng đã thanh lý, không thể gửi yêu cầu ký kết.");

        if (!contract.NgayKy.HasValue)
            throw new BusinessRuleException("Hợp đồng chưa có ngày ký, vui lòng cập nhật trước khi gửi.");

        var customer = await _customerRepository.GetByIdEnrichedAsync(contract.KhachHangId, ct)
            ?? throw new NotFoundException(nameof(KhachHang), contract.KhachHangId);

        if (string.IsNullOrWhiteSpace(customer.Email))
            throw new BusinessRuleException("Khách hàng chưa có email trong hồ sơ, không thể gửi hợp đồng.");

        // Chặn gửi trùng liên tiếp trong thời gian ngắn.
        var lichSu = await _loyaltyRepository.LayLichSuGuiEmailAsync(
            contract.KhachHangId, "HopDong", contract.MaHopDong, ct);
        var lanGuiGanNhat = lichSu.Where(x => x.ThanhCong).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
        if (lanGuiGanNhat.CreatedAt.HasValue
            && DateTime.UtcNow - lanGuiGanNhat.CreatedAt.Value < KhoangCachToiThieu)
        {
            var conLai = KhoangCachToiThieu - (DateTime.UtcNow - lanGuiGanNhat.CreatedAt.Value);
            throw new BusinessRuleException(
                $"Hợp đồng {contract.MaHopDong} vừa được gửi cách đây chưa đầy 2 phút. " +
                $"Vui lòng đợi thêm khoảng {Math.Ceiling(conLai.TotalSeconds / 60)} phút rồi thử lại " +
                "để tránh gửi trùng cho khách.");
        }

        var sanPham = new List<ContractPdfProductLine>();
        if (contract.BaoGiaId.HasValue)
        {
            var chiTiet = await _quoteRepository.GetChiTietAsync(contract.BaoGiaId.Value, ct);
            sanPham = chiTiet.Select(l => new ContractPdfProductLine
            {
                TenSP = l.TenSP ?? "",
                DonVi = l.DonVi,
                SoLuong = l.SoLuong,
                DonGia = l.DonGia,
                ThanhTien = l.ThanhTien,
            }).ToList();
        }

        var lichThanhToan = new List<ContractPdfLichThanhToan>();
        if (contract.HinhThucThanhToan != HINH_THUC_THANH_TOAN_MOT_LAN)
        {
            var lich = await _contractRepository.GetLichThanhToanByHopDongAsync(cmd.HopDongId, ct);
            lichThanhToan = lich.Select(l => new ContractPdfLichThanhToan
            {
                SoDot = l.SoDot,
                SoTien = l.SoTien,
                HanThanhToan = l.HanThanhToan,
            }).ToList();
        }

        var giaTriHopDong = contract.GiaTri ?? sanPham.Sum(l => l.ThanhTien);
        if (giaTriHopDong <= 0)
            throw new BusinessRuleException(
                "Hợp đồng chưa có giá trị (hoặc chưa có sản phẩm/dịch vụ nào), không thể gửi.");

        var pdfModel = new ContractPdfModel
        {
            MaHopDong = contract.MaHopDong,
            NgayKy = contract.NgayKy,
            ThoiHan = contract.ThoiHan,
            NgayKetThuc = contract.NgayKetThuc,
            HinhThucThanhToanLabel = contract.HinhThucThanhToan == HINH_THUC_THANH_TOAN_MOT_LAN
                ? "Thanh toán một lần"
                : "Thanh toán thành nhiều đợt (trả góp định kỳ)",
            GiaTriHopDong = giaTriHopDong,
            TenKhachHang = customer.TenKhachHang,
            KhachMaSoThue = customer.MaSoThue,
            KhachDienThoai = customer.SoDienThoai,
            KhachEmail = customer.Email,
            SanPham = sanPham,
            LichThanhToan = lichThanhToan,
            BenA = req.BenA,
            BenB = req.BenB,
            DiaDiemKy = req.DiaDiemKy,
            VatIncluded = req.VatIncluded,
            BaoHanhThang = req.BaoHanhThang,
            MucPhatViPham = req.MucPhatViPham,
            SoBan = req.SoBan,
            ClauseTexts = req.ClauseTexts,
        };

        var pdfBytes = _pdfGenerator.Generate(pdfModel);

        // Audit: không có cột DB riêng cho "ai đã gửi", nên ghi vào log ứng dụng (Serilog) —
        // đủ để truy vết trách nhiệm khi cần đối chiếu, không cần đổi schema.
        _logger.LogInformation(
            "Nhân viên {NhanSuId} ({Username}) gửi hợp đồng {MaHopDong} tới khách hàng {KhachHangId} ({Email})",
            _currentUser.NhanSuId, _currentUser.Username, contract.MaHopDong, customer.Id, customer.Email);

        var (thanhCong, loiChiTiet) = await _emailService.GuiEmailHopDongAsync(
            customer.Id, customer.TenKhachHang, customer.Email,
            contract.MaHopDong, pdfBytes, req.LoiNhan, ct);

        if (!thanhCong)
            throw new BusinessRuleException($"Gửi email thất bại: {loiChiTiet ?? "lỗi không xác định"}.");

        return new SendContractEmailResultDto
        {
            ThanhCong = true,
            ThoiGianGui = DateTime.UtcNow,
            EmailDaGui = customer.Email,
        };
    }
}
