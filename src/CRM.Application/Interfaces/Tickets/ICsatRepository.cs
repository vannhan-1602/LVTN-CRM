using CRM.Application.Features.Tickets.DTOs;

namespace CRM.Application.Interfaces.Tickets;

/// <summary>Khảo sát mức độ hài lòng (CSAT) sau khi ticket đóng — tương tự cơ chế QuotePublicTokenService,
/// nhưng token được lưu trực tiếp trong TK_DanhGiaHaiLong.Token (unique) thay vì tính bằng HMAC.</summary>
public interface ICsatRepository
{
    /// <summary>Tạo yêu cầu đánh giá cho 1 ticket vừa đóng, trả về token public.</summary>
    Task<string> CreateRequestAsync(ulong ticketId, CancellationToken ct = default);

    Task MarkEmailSentAsync(ulong ticketId, CancellationToken ct = default);

    Task<CsatDto?> GetByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Ghi nhận đánh giá của khách qua token. Trả về null nếu token không tồn tại hoặc đã đánh giá rồi.</summary>
    Task<CsatDto?> SubmitAsync(string token, byte diemDanhGia, string? nhanXet, CancellationToken ct = default);
}
