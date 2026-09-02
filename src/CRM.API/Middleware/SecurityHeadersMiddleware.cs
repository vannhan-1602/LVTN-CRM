namespace CRM.API.Middleware;


public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Chặn trình duyệt tự đoán (MIME-sniff) content-type khác với Content-Type server khai
        // báo — ngăn 1 file upload giả dạng ảnh nhưng thực chất là HTML/JS chạy được.
        headers["X-Content-Type-Options"] = "nosniff";

        // Chặn API/Swagger UI bị nhúng vào <iframe> ở domain khác — phòng clickjacking.
        headers["X-Frame-Options"] = "DENY";

        // Không gửi full URL (có thể chứa id nội bộ, token trong query string) sang domain khác
        // khi người dùng click 1 link ra ngoài — chỉ gửi origin, không gửi path/query.
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Tắt hẳn các API trình duyệt nhạy cảm mà 1 REST API JSON chắc chắn không cần dùng tới.
        headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        // HSTS: chỉ set khi request đã qua HTTPS — set trên HTTP sẽ vô nghĩa (header bị bỏ qua)
        // và set nhầm trong môi trường dev (chạy HTTP) có thể khiến trình duyệt "khoá cứng"
        // localhost sang HTTPS trong lần truy cập sau, gây khó chịu khi dev. max-age 180 ngày —
        // đủ dài để có tác dụng bảo vệ, không dài tới mức không thể đổi ý nếu cần rollback.
        if (context.Request.IsHttps)
        {
            headers["Strict-Transport-Security"] = "max-age=15552000; includeSubDomains";
        }

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();
}