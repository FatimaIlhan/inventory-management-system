using Api.Middleware;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddTransient<GlobalExceptionMiddleware>();
        services.AddHealthChecks();

        return services;
    }
}
