using Api.Extensions;
using Api.DTOs;
using Infrastructure.Authentication;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var details = context.ModelState
                .Where(entry => entry.Value is { Errors.Count: > 0 })
                .SelectMany(entry => entry.Value!.Errors.Select(error =>
                    string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? $"{entry.Key}: The supplied value is invalid."
                        : $"{entry.Key}: {error.ErrorMessage}"))
                .ToArray();

            return new BadRequestObjectResult(new ApiErrorResponse(
                context.HttpContext.TraceIdentifier,
                "validation_failed",
                "One or more validation errors occurred.",
                details));
        };
    });
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var angularOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:4200";

builder.Services.AddCors(options =>
{
	options.AddPolicy("AngularClient", policy =>
	{
		policy.WithOrigins(angularOrigin)
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

var app = builder.Build();

if (args.Contains("--provision-admin", StringComparer.Ordinal))
{
    using var scope = app.Services.CreateScope();
    var provisioner = scope.ServiceProvider.GetRequiredService<AdminProvisioningService>();
    var result = await provisioner.ProvisionAsync(app.Lifetime.ApplicationStopping);
    app.Logger.LogInformation("Administrator provisioning completed with result {Result}.", result);
    return;
}

app.UseApiPipeline();

app.Run();
