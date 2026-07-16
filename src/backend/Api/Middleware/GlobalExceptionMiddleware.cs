using System.Net;
using System.Text.Json;
using Application.Exceptions;
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

            var (statusCode, message) = exception switch
            {
                AppValidationException validationException => (HttpStatusCode.BadRequest, validationException.Message),
                UnauthorizedException unauthorizedException => (HttpStatusCode.Unauthorized, unauthorizedException.Message),
                NotFoundException notFoundException => (HttpStatusCode.NotFound, notFoundException.Message),
                _ => (HttpStatusCode.InternalServerError, "Internal server error.")
            };

            var response = ApiResponse<object>.Fail(message, statusCode == HttpStatusCode.InternalServerError ? errors : null);
            var json = JsonSerializer.Serialize(response);

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(json);
        }
    }
}
