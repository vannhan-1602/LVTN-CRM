using CRM.Application;
using CRM.Infrastructure;
using CRM.API.Extensions;
using CRM.API.Middleware;
using Serilog;

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:5174")
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

    options.AddFixedWindowLimiter("PublicFormSubmit", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});

var app = builder.Build();

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
app.MapControllers();

app.Run();