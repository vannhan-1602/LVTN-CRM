using CRM.Application;
using CRM.Infrastructure;
using CRM.API.Extensions;
using CRM.API.Middleware;
using CRM.API.Hubs;
using CRM.Application.Interfaces.Notifications;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CRM Online API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập JWT token: Bearer {token}"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthPolicies();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

// Output cache cho các endpoint đọc nhiều, đổi ít (danh mục sản phẩm, loại KH, tỉnh/thành...) —
// đăng ký sẵn policy để controller gắn [OutputCache(PolicyName = "DanhMuc")] khi cần, có eviction
// theo tag để invalidate ngay khi dữ liệu bên dưới thay đổi thay vì chỉ chờ hết TTL.
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("DanhMuc", policy => policy.Expire(TimeSpan.FromMinutes(10)).Tag("danh-muc"));
    options.AddPolicy("Dashboard", policy => policy.Expire(TimeSpan.FromMinutes(2)).Tag("dashboard"));
});

// Health check tự viết bằng CanConnectAsync thay vì gói NuGet
// Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore — tránh thêm dependency
// ngoài chỉ cho một lệnh "SELECT 1" mà DbContext đã tự làm được.
builder.Services.AddHealthChecks()
    .AddCheck<CRM.API.Extensions.DatabaseHealthCheck>("database");

// SignalR — cho phép backend chủ động đẩy sự kiện xuống client (thông báo, cập nhật số liệu)
// thay vì client phải polling/F5. Xem CRM.API/Hubs/NotificationHub.cs.
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationPublisher, SignalRNotificationPublisher>();

// Production chạy sau Caddy (reverse proxy) — request thật tới backend luôn là HTTP nội bộ
// trong mạng Docker, TLS được Caddy terminate ở biên. Nếu không forward header, ASP.NET Core
// sẽ luôn thấy: IsHttps = false (khiến middleware HSTS phía trên không bao giờ set header),
// và Connection.RemoteIpAddress = IP của container Caddy thay vì IP client thật (khiến rate
// limiter "LoginAttempt" gộp TẤT CẢ người dùng vào chung 1 bucket theo IP Caddy, và
// CreatedByIp lưu trong HT_RefreshToken vô nghĩa cho việc audit).
//
// KnownNetworks/KnownProxies để rỗng (thay vì chỉ định IP Caddy cụ thể) vì: (1) container Caddy
// không có IP cố định giữa các lần deploy/restart trên Docker network, và (2) theo thiết kế
// compose hiện tại, backend KHÔNG public port ra ngoài — chỉ Caddy trong cùng mạng Docker mới
// gọi vào được (xem docker-compose.prod.yml), nên tin tưởng X-Forwarded-* ở đây không mở thêm
// đường tấn công nào so với hiện trạng.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Đọc từ config "AllowedFrontendOrigins" (env var AllowedFrontendOrigins__0, __1, ...)
        // để domain Vercel thật không bị hardcode vào code — chỉ fallback về localhost khi
        // chưa cấu hình (dev máy local).
        var origins = builder.Configuration
            .GetSection("AllowedFrontendOrigins")
            .Get<string[]>() ?? new[] { "http://localhost:5173", "http://localhost:5174" };

        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    // Policy riêng, lỏng hơn, chỉ áp cho nhóm route api/public/* (landing page — không có cookie/token
    // nên không cần AllowCredentials). Thêm domain landing page thật vào đây khi build/deploy xong.
    options.AddPolicy("AllowPublicForms", policy =>
    {
        var publicOrigins = builder.Configuration
            .GetSection("PublicFormsAllowedOrigins")
            .Get<string[]>() ?? new[] { "http://localhost:5173", "http://localhost:5174" };

        policy
            .WithOrigins(publicOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Chống spam cho các endpoint public (AllowAnonymous) như api/public/leads — giới hạn theo IP.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("PublicFormSubmit", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Chống brute-force đăng nhập: giới hạn theo IP, tách riêng (chặt hơn) khỏi
    // PublicFormSubmit vì login là mục tiêu tấn công phổ biến nhất. Không lock account theo
    // username trong DB (tránh bị lợi dụng để khóa tài khoản người khác hàng loạt chỉ bằng
    // cách gõ sai mật khẩu — "account lockout DoS") — chỉ giới hạn theo IP ở tầng middleware.
    options.AddPolicy("LoginAttempt", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// Phải đứng ĐẦU pipeline (trước mọi middleware đọc IsHttps/RemoteIpAddress) — SecurityHeaders
// (đọc IsHttps) và rate limiter (đọc RemoteIpAddress) đứng sau nên nhận được giá trị đã sửa.
app.UseForwardedHeaders();

app.UseSecurityHeaders();

// Nén response (Brotli ưu tiên, fallback Gzip) — giảm băng thông đáng kể cho các payload JSON
// lớn (danh sách khách hàng, báo giá, dashboard...). Phải đặt SỚM trong pipeline (ngay sau khi
// build, trước khi ghi bất kỳ response nào) để bọc được toàn bộ response body phía sau.
app.UseResponseCompression();

app.UseSerilogRequestLogging();
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseRateLimiter();

// Phục vụ file tĩnh trong wwwroot (ảnh sản phẩm upload lên /uploads/products/...)
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads", "products");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

// Health check đơn giản cho deploy/monitoring (uptime checker, load balancer, CI/CD gate).
// "/health" không yêu cầu Authorize — nằm ngoài mọi rate limit vì được gọi rất thường xuyên
// bởi hệ thống giám sát, không phải người dùng cuối.
app.MapHealthChecks("/health");

app.Run();