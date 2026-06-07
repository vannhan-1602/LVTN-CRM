using System.Net;
using System.Text.Json;
using CRM.Application.Common.Exceptions;
using CRM.Application.Common.Models;
using AppValidationException = CRM.Application.Common.Exceptions.ValidationException;

namespace CRM.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = MapException(exception);

        if ((int)statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        if (!_environment.IsDevelopment() && (int)statusCode >= 500)
        {
            response = ApiResponse.Fail("An unexpected error occurred.");
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static (HttpStatusCode StatusCode, ApiResponse Response) MapException(Exception exception) =>
        exception switch
        {
            AppValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ApiResponse.Fail(
                    validationEx.Message,
                    validationEx.Errors.SelectMany(e => e.Value))),

            FluentValidation.ValidationException fluentEx => (
                HttpStatusCode.BadRequest,
                ApiResponse.Fail(
                    "Validation failed.",
                    fluentEx.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))),

            NotFoundException => (
                HttpStatusCode.NotFound,
                ApiResponse.Fail(exception.Message)),

            ForbiddenException => (
                HttpStatusCode.Forbidden,
                ApiResponse.Fail(exception.Message)),

            UnauthorizedException => (
                HttpStatusCode.Unauthorized,
                ApiResponse.Fail(exception.Message)),

            BusinessRuleException => (
                HttpStatusCode.UnprocessableEntity,
                ApiResponse.Fail(exception.Message)),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ApiResponse.Fail("Unauthorized.")),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse.Fail(exception.Message))
        };
}

public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app) =>
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
}
