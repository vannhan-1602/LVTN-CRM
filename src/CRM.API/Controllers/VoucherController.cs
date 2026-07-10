using CRM.Application.Features.Loyalty.Commands.RedeemVoucher;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

// Public — khách bấm link trong email, không đăng nhập vào hệ thống CRM.
[ApiController]
[Route("api/voucher")]
public class VoucherController : ControllerBase
{
    private readonly IMediator _mediator;
    public VoucherController(IMediator mediator) => _mediator = mediator;

    [HttpGet("redeem")]
    public async Task<ContentResult> Redeem([FromQuery] string token, CancellationToken ct)
    {
        var result = await _mediator.Send(new RedeemVoucherCommand(token), ct);
        return Content(BuildHtml(result), "text/html; charset=utf-8");
    }

    private static string BuildHtml(RedeemVoucherResult r)
    {
        var icon = r.Success ? "✅" : "⚠️";
        var color = r.Success ? "#16a34a" : "#dc2626";
        var detail = r.Success
            ? $"<div style=\"margin-top:16px;padding:16px;background:#f8fafc;border-radius:12px;text-align:left\">" +
              $"<p style=\"margin:0 0 6px;color:#64748b;font-size:13px\">Mã voucher</p>" +
              $"<p style=\"margin:0 0 14px;font-weight:600;font-size:18px;color:#0f172a\">{r.MaVoucher}</p>" +
              $"<p style=\"margin:0 0 6px;color:#64748b;font-size:13px\">Ưu đãi</p>" +
              $"<p style=\"margin:0 0 14px;font-weight:600;color:#0f172a\">Giảm {r.GiaTriGiam}{(r.LoaiGiamGia == "PhanTram" ? "%" : "đ")}" +
              $"{(r.GiaTriGiamToiDa.HasValue ? $" (tối đa {r.GiaTriGiamToiDa:N0}đ)" : "")}</p>" +
              $"<p style=\"margin:0;color:#64748b;font-size:13px\">Hạn sử dụng: {r.NgayHetHan:dd/MM/yyyy}</p>" +
              $"</div>"
            : "";

        return "<!DOCTYPE html><html lang=\"vi\"><head><meta charset=\"utf-8\" />" +
               "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />" +
               "<title>Xác nhận ưu đãi</title></head>" +
               "<body style=\"margin:0;background:#f1f5f9;font-family:-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif;\">" +
               "<div style=\"max-width:420px;margin:60px auto;background:#fff;border-radius:16px;padding:32px 28px;text-align:center;box-shadow:0 1px 3px rgba(0,0,0,0.08)\">" +
               $"<div style=\"font-size:40px\">{icon}</div>" +
               $"<h1 style=\"font-size:18px;color:{color};margin:14px 0 8px\">{(r.Success ? "Xác nhận thành công" : "Không thể xác nhận")}</h1>" +
               $"<p style=\"color:#475569;font-size:14px;line-height:1.6;margin:0\">{r.Message}</p>" +
               detail +
               "</div></body></html>";
    }
}
