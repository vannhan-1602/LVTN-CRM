using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Loyalty;
using CRM.Application.Interfaces.Tickets;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Loyalty.Commands.RedeemVoucher;

public class RedeemVoucherCommandHandler : IRequestHandler<RedeemVoucherCommand, RedeemVoucherResult>
{
    private const string LoaiTicketVoucher = "Yêu cầu sử dụng Voucher"; // TK_LoaiTicket seed sẵn Id=4
    private const string AuditTable = "TK_Ticket";

    private readonly ILoyaltyRepository _repo;
    private readonly ITicketRepository _ticketRepository;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<RedeemVoucherCommandHandler> _logger;

    public RedeemVoucherCommandHandler(
        ILoyaltyRepository repo,
        ITicketRepository ticketRepository,
        IAuditLogPublisher auditLogPublisher,
        ILogger<RedeemVoucherCommandHandler> logger)
    {
        _repo = repo;
        _ticketRepository = ticketRepository;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<RedeemVoucherResult> Handle(RedeemVoucherCommand request, CancellationToken ct)
    {
        var (voucher, token) = await _repo.GetVoucherByTokenAsync(request.Token, ct);

        if (token is null)
            return Fail("Link không hợp lệ hoặc không tồn tại.");

        if (token.DaSuDung)
            return Fail("Link này đã được sử dụng trước đó. Nếu cần hỗ trợ thêm, vui lòng liên hệ tổng đài.");

        if (token.NgayHetHanToken < DateTime.UtcNow)
            return Fail("Link đã hết hạn. Vui lòng liên hệ để được hỗ trợ.");

        if (voucher is null)
            return Fail("Không tìm thấy voucher tương ứng.");

        if (voucher.IsUsed)
            return Fail("Voucher này đã được sử dụng trước đó.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (voucher.NgayHetHan < today)
            return Fail("Voucher đã hết hạn sử dụng.");

        await _repo.DanhDauTokenDaSuDungAsync(token.Id, ct);

        // Tạo Ticket "Yêu cầu sử dụng Voucher" để nhân viên chủ động liên hệ hỗ trợ khách áp dụng
        // ưu đãi vào báo giá/hóa đơn gần nhất, rồi gắn Ticket_Id vào voucher để truy vết 2 chiều.
        // Nếu bước này lỗi (vd. mất kết nối DB tạm thời), vẫn coi redemption là thành công với khách,
        // chỉ cần fallback đánh dấu "đã yêu cầu" để không làm gián đoạn trải nghiệm người dùng cuối.
        try
        {
            var loaiTicketId = await _ticketRepository.GetLoaiTicketIdByNameAsync(LoaiTicketVoucher, ct);
            var ticketId = await _ticketRepository.CreateTicketForVoucherAsync(
                voucher.KhachHangId,
                loaiTicketId,
                $"Khách hàng xác nhận sử dụng voucher {voucher.MaVoucher}",
                $"Khách hàng đã bấm link trong email xác nhận muốn sử dụng voucher {voucher.MaVoucher} " +
                $"(giảm {voucher.GiaTriGiam}{(voucher.LoaiGiamGia == "PhanTram" ? "%" : "đ")}" +
                $"{(voucher.GiaTriGiamToiDa.HasValue ? $", tối đa {voucher.GiaTriGiamToiDa:N0}đ" : "")}, " +
                $"hạn dùng đến {voucher.NgayHetHan:dd/MM/yyyy}). " +
                "Vui lòng liên hệ khách hàng để hỗ trợ áp dụng vào báo giá/hóa đơn gần nhất.",
                ct);

            await _repo.GanTicketVaoVoucherAsync(voucher.Id, ticketId, ct);

            try
            {
                await _auditLogPublisher.PublishAsync(AuditTable, ticketId, "INSERT",
                    oldData: null, newData: new { voucher.MaVoucher, voucher.KhachHangId, ticketId }, ct);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for voucher ticket {Ticket}", ticketId); }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Voucher] Lỗi tạo ticket cho voucher {Voucher}, fallback đánh dấu đã yêu cầu", voucher.MaVoucher);
            await _repo.DanhDauDaYeuCauAsync(voucher.Id, ct);
        }

        return new RedeemVoucherResult
        {
            Success = true,
            Message = "Xác nhận thành công! Nhân viên tư vấn sẽ liên hệ hỗ trợ bạn áp dụng ưu đãi trong đơn hàng gần nhất.",
            MaVoucher = voucher.MaVoucher,
            LoaiGiamGia = voucher.LoaiGiamGia,
            GiaTriGiam = voucher.GiaTriGiam,
            GiaTriGiamToiDa = voucher.GiaTriGiamToiDa,
            NgayHetHan = voucher.NgayHetHan
        };
    }

    private static RedeemVoucherResult Fail(string message) => new() { Success = false, Message = message };
}