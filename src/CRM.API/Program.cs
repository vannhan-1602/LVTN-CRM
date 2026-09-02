using CRM.Application;
using CRM.Infrastructure;
using CRM.API.Extensions;
using CRM.API.Middleware;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// appsettings.Local.json chứa các secret thật (SMTP password, connection string thật...) —
// đã có trong .gitignore, không commit lên git. Nếu file không tồn tại (vd: máy CI/máy khác),
// optional:true nên vẫn chạy bình thường, chỉ là dùng giá trị placeholder trong appsettings.json.
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

// Nén Brotli/Gzip cho cả HTTPS (mặc định ASP.NET Core tắt nén trên HTTPS để phòng BREACH
// attack trên các trang nhạy cảm với secret trong body — API JSON thuần không rơi vào nhóm
// rủi ro đó nên bật lại an toàn, đổi lại giảm đáng kể băng thông/độ trễ).
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

// Health check đơn giản cho deploy/monitoring (uptime checker, load balancer, CI/CD gate).
// "/health" không yêu cầu Authorize — nằm ngoài mọi rate limit vì được gọi rất thường xuyên
// bởi hệ thống giám sát, không phải người dùng cuối.
app.MapHealthChecks("/health");

app.Run();