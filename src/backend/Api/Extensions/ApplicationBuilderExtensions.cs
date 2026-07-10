namespace Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors("AngularClient");
        app.UseMiddleware<Api.Middleware.GlobalExceptionMiddleware>();

        app.UseAuthorization();

        app.MapControllers();
        app.MapGet("/api/health", () => Results.Ok("OK"));

        return app;
    }
}
