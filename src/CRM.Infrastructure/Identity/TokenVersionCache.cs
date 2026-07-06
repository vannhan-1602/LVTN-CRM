using Microsoft.Extensions.Caching.Memory;

namespace CRM.Infrastructure.Identity;

/// <summary>
/// Cache ngắn hạn (TTL nhỏ) cho (TokenVersion, TrangThai) của user, dùng bởi middleware xác thực
/// JWT để tránh phải query DB trên MỌI request (bug gốc: "không có middleware re-check DB").
///
/// Khi tài khoản bị khóa / đổi vai trò / đổi mật khẩu, UserManagementRepository chủ động gọi
/// Invalidate() ngay lập tức — nên việc thu hồi có hiệu lực tức thời, không phải chờ hết TTL.
/// TTL chỉ đóng vai trò dự phòng cho các thay đổi không đi qua repository này (vd sửa thẳng DB).
/// </summary>
public class TokenVersionCache
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    public TokenVersionCache(IMemoryCache cache) => _cache = cache;

    private static string Key(uint userId) => $"tokenversion:{userId}";

    public bool TryGet(uint userId, out (int TokenVersion, string TrangThai) value) =>
        _cache.TryGetValue(Key(userId), out value);

    public void Set(uint userId, int tokenVersion, string trangThai) =>
        _cache.Set(Key(userId), (tokenVersion, trangThai), Ttl);

    public void Invalidate(uint userId) => _cache.Remove(Key(userId));
}