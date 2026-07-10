using MediatR;

namespace CRM.Application.Features.Loyalty.Commands.RedeemVoucher;

public record RedeemVoucherCommand(string Token) : IRequest<RedeemVoucherResult>;

public class RedeemVoucherResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MaVoucher { get; set; }
    public string? LoaiGiamGia { get; set; }
    public decimal? GiaTriGiam { get; set; }
    public decimal? GiaTriGiamToiDa { get; set; }
    public DateOnly? NgayHetHan { get; set; }
}
