using System.Net;
using System.Text.Json;
using Api.DTOs;

namespace Api.Middleware;

public sealed class GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment hostEnvironment) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception occurred while processing request {Path}.", context.Request.Path);

            var errors = hostEnvironment.IsDevelopment()
                ? new[] { exception.Message, exception.StackTrace ?? string.Empty }
                : new[] { "An unexpected error occurred." };

            var response = ApiResponse<object>.Fail("Internal server error.", errors);
            var json = JsonSerializer.Serialize(response);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(json);
        }
    }
}
