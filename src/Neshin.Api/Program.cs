using FastEndpoints;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using FastEndpoints.Swagger;
using Neshin.Application.Common;
using Neshin.Domain.Common;
using Neshin.Infrastructure.CustomerExperience;
using Neshin.Infrastructure.Persistence;
using Neshin.Infrastructure.Persistence.Repositories;
using Neshin.Api.Http;
using Neshin.Application.Abstractions.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddProblemDetails();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestContext, RequestContext>();
builder.Services.AddFastEndpoints();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("customer-session", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
    options.AddPolicy("discovery", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
    options.AddPolicy("customer-write", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
    options.AddPolicy("public-read", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
    options.AddPolicy("owner", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

var writeConnectionString = builder.Configuration.GetConnectionString("Write")
    ?? throw new InvalidOperationException("Connection string 'Write' is not configured.");

var readConnectionString = builder.Configuration.GetConnectionString("Read")
    ?? throw new InvalidOperationException("Connection string 'Read' is not configured.");

builder.Services.AddDbContext<NeshinWriteDbContext>(options =>
    options.UseNpgsql(writeConnectionString, npgsql =>
        npgsql.MigrationsAssembly(typeof(NeshinWriteDbContext).Assembly.FullName)));

builder.Services.AddDbContext<NeshinReadDbContext>(options => options.UseNpgsql(readConnectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserWriteRepository, UserWriteRepository>();
builder.Services.AddScoped<IUserReadRepository, UserReadRepository>();
builder.Services.AddScoped<IPublicExperienceRepository, PublicExperienceRepository>();
builder.Services.AddScoped<ICustomerOrderRepository, CustomerOrderRepository>();
builder.Services.AddScoped<IOwnerExperienceRepository, OwnerExperienceRepository>();
builder.Services.AddScoped<IUserExperienceRepository, UserExperienceRepository>();
builder.Services.AddScoped<IClientExperienceRepository, ClientExperienceRepository>();
builder.Services.AddScoped<IOtpVerifier, ConfigurationOtpVerifier>();

builder.Services.SwaggerDocument(settings =>
{
    settings.DocumentSettings = generatorSettings =>
    {
        generatorSettings.Title = "Neshin - WebApi";
        generatorSettings.DocumentName = "v1";
        generatorSettings.Version = "v1";
    };
    settings.EnableJWTBearerAuth = false;
    settings.MaxEndpointVersion = 1;
    settings.MinEndpointVersion = 1;
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseRateLimiter();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var statusCode = exception switch
        {
            DomainException => StatusCodes.Status400BadRequest,
            RequestUnauthorizedException => StatusCodes.Status401Unauthorized,
            ResourceNotFoundException => StatusCodes.Status404NotFound,
            RequestConflictException => StatusCodes.Status409Conflict,
            FeatureNotAvailableException => StatusCodes.Status501NotImplemented,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;
        await Results.Problem(
                statusCode: statusCode,
                title: statusCode switch
                {
                    StatusCodes.Status400BadRequest => "Business rule violation",
                    StatusCodes.Status401Unauthorized => "Unauthorized",
                    StatusCodes.Status404NotFound => "Resource not found",
                    StatusCodes.Status409Conflict => "Request conflict",
                    StatusCodes.Status501NotImplemented => "Feature not available",
                    _ => "Unexpected server error"
                },
                detail: statusCode < 500 ? exception?.Message : null)
            .ExecuteAsync(context);
    });
});

app.UseFastEndpoints(options =>
{
    options.Versioning.Prefix = "v";
    options.Versioning.RouteTemplate = "{version}";
});

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.Run();

public partial class Program;
