using System.Text.Json;
using Application.Exceptions;
using Api.DTOs;

namespace Api.Middleware;

public sealed class GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger) : IMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (statusCode, code, message) = exception switch
            {
                AppValidationException validationException => (StatusCodes.Status400BadRequest, "validation_failed", validationException.Message),
                UnauthorizedException unauthorizedException => (StatusCodes.Status401Unauthorized, "unauthorized", unauthorizedException.Message),
                ConflictException conflictException => (StatusCodes.Status409Conflict, "conflict", conflictException.Message),
                NotFoundException notFoundException => (StatusCodes.Status404NotFound, "not_found", notFoundException.Message),
                _ => (StatusCodes.Status500InternalServerError, "internal_error", "Internal server error.")
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception while processing {Method} {Path}. TraceId: {TraceId}", context.Request.Method, context.Request.Path, context.TraceIdentifier);
            }
            else
            {
                logger.LogWarning("Request failed with {StatusCode} at {Method} {Path}. TraceId: {TraceId}. Reason: {Reason}", statusCode, context.Request.Method, context.Request.Path, context.TraceIdentifier, exception.Message);
            }

            var details = exception is AppValidationException
                ? new[] { exception.Message }
                : null;
            var response = new ApiErrorResponse(context.TraceIdentifier, code, message, details);
            var json = JsonSerializer.Serialize(response, SerializerOptions);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(json);
        }
    }
}
