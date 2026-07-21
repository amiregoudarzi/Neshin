using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Neshin.Api.Endpoints;
using Neshin.Application.Abstractions.Persistence;
using Neshin.Application.Users.Queries.GetUserById;
using Neshin.Domain.Common;
using Neshin.Infrastructure.Persistence;
using Neshin.Infrastructure.Persistence.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(TimeProvider.System);

var connectionString = builder.Configuration.GetConnectionString("NeshinDatabase")
    ?? throw new InvalidOperationException("Connection string 'NeshinDatabase' is not configured.");

builder.Services.AddDbContext<NeshinWriteDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsAssembly(typeof(NeshinWriteDbContext).Assembly.FullName)));

builder.Services.AddDbContext<NeshinReadDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserWriteRepository, UserWriteRepository>();
builder.Services.AddScoped<IUserReadRepository, UserReadRepository>();
builder.Services.AddScoped<GetUserByIdQueryHandler>();


var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var statusCode = exception is DomainException
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;

        context.Response.StatusCode = statusCode;
        await Results.Problem(
                statusCode: statusCode,
                title: exception is DomainException ? "Business rule violation" : "Unexpected server error",
                detail: exception is DomainException ? exception.Message : null)
            .ExecuteAsync(context);
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Neshin.Api" }))
    .WithName("Health")
    .WithTags("System");

app.MapUserEndpoints();

app.Run();

public partial class Program;
