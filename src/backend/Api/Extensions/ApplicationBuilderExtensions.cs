using Api.DTOs;

namespace Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors("AngularClient");
        app.UseMiddleware<Api.Middleware.GlobalExceptionMiddleware>();
        app.UseStatusCodePages(async statusCodeContext =>
        {
            var response = statusCodeContext.HttpContext.Response;
            var (code, message) = response.StatusCode switch
            {
                StatusCodes.Status400BadRequest => ("bad_request", "The request is invalid."),
                StatusCodes.Status401Unauthorized => ("unauthorized", "Authentication is required."),
                StatusCodes.Status403Forbidden => ("forbidden", "You do not have permission to perform this action."),
                StatusCodes.Status404NotFound => ("not_found", "The requested resource was not found."),
                _ when response.StatusCode >= StatusCodes.Status500InternalServerError => ("internal_error", "Internal server error."),
                _ => ("request_failed", "The request could not be completed.")
            };

            await response.WriteAsJsonAsync(new ApiErrorResponse(
                statusCodeContext.HttpContext.TraceIdentifier,
                code,
                message));
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapGet("/api/health", () => Results.Ok("OK"));

        return app;
    }
}
