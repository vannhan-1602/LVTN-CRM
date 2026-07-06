using System.Security.Claims;
using System.Text.Json;
using CRM.Application.Common.Models;
using CRM.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Infrastructure.Identity;

public static class JwtBearerEventHandlers
{
    public static JwtBearerEvents Create() => new()
    {
        // Thu hồi JWT tức thời: JWT mặc định chỉ hết hạn theo thời gian (60 phút) và không có
        // cách hủy giữa chừng. Handler này so khớp claim "tv" (TokenVersion) trong token với
        // giá trị hiện tại trong DB (qua cache ngắn hạn) trên MỌI request đã xác thực chữ ký —
        // nếu tài khoản vừa bị khóa / đổi vai trò / đổi mật khẩu (TokenVersion đã tăng), hoặc
        // tài khoản không còn Active, token cũ bị từ chối ngay dù chữ ký vẫn hợp lệ và chưa hết hạn.
        OnTokenValidated = async context =>
        {
            var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var tvClaim = context.Principal?.FindFirstValue("tv");

            if (userIdClaim is null || !uint.TryParse(userIdClaim, out var userId))
            {
                context.Fail("Token không hợp lệ.");
                return;
            }

            var tokenTv = int.TryParse(tvClaim, out var parsedTv) ? parsedTv : 0;

            var cache = context.HttpContext.RequestServices.GetRequiredService<TokenVersionCache>();

            (int TokenVersion, string TrangThai) current;
            if (!cache.TryGet(userId, out current))
            {
                var userRepo = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var fromDb = await userRepo.GetTokenVersionAsync(userId, context.HttpContext.RequestAborted);

                if (fromDb is null)
                {
                    context.Fail("Tài khoản không còn tồn tại.");
                    return;
                }

                current = fromDb.Value;
                cache.Set(userId, current.TokenVersion, current.TrangThai);
            }

            if (current.TrangThai != "Active")
            {
                context.Fail("Tài khoản đang bị khóa hoặc vô hiệu hóa.");
                return;
            }

            if (current.TokenVersion != tokenTv)
            {
                context.Fail("Phiên đăng nhập đã bị thu hồi, vui lòng đăng nhập lại.");
                return;
            }
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();

            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = ApiResponse.Fail("Unauthorized. Vui lòng đăng nhập để truy cập tài nguyên này.");
            await context.Response.WriteAsync(Serialize(response));
        },
        OnForbidden = async context =>
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var response = ApiResponse.Fail("Forbidden. Bạn không có quyền truy cập tài nguyên này.");
            await context.Response.WriteAsync(Serialize(response));
        }
    };

    private static string Serialize(ApiResponse response) =>
        JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
}