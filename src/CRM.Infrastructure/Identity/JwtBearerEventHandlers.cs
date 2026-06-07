using System.Text.Json;
using CRM.Application.Common.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace CRM.Infrastructure.Identity;

public static class JwtBearerEventHandlers
{
    public static JwtBearerEvents Create() => new()
    {
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
