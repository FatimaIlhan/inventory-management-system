using Api.Extensions;
using Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiServices();
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

app.UseApiPipeline();

app.Run();
