namespace CRM.Application.Interfaces.Quotes;

/// <summary>
/// Sinh & xác thực token công khai cho báo giá — dùng để khách hàng bấm link
/// trong email xem/chấp nhận/từ chối báo giá mà KHÔNG cần đăng nhập vào CRM.
///
/// Token được tính bằng HMAC-SHA256 (chữ ký số) từ QuoteId + khóa bí mật của hệ
/// thống, KHÔNG lưu vào DB — xác thực bằng cách tính lại chữ ký và so sánh.
/// Nhờ vậy không cần thêm cột/bảng mới.
/// </summary>
public interface IQuotePublicTokenService
{
    string GenerateToken(ulong quoteId);

    //Trả về QuoteId nếu token hợp lệ, ngược lại null.
    ulong? ValidateToken(string token);
}