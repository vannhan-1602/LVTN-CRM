using CRM.Application.Interfaces.Loyalty;
using MediatR;

namespace CRM.Application.Features.Loyalty.Commands.RedeemVoucher;

public class RedeemVoucherCommandHandler : IRequestHandler<RedeemVoucherCommand, RedeemVoucherResult>
{
    private readonly ILoyaltyRepository _repo;
    public RedeemVoucherCommandHandler(ILoyaltyRepository repo) => _repo = repo;

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
        await _repo.DanhDauDaYeuCauAsync(voucher.Id, ct);

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
