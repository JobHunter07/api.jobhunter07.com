using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Reflection;
using VerticalSliceArchitectureTemplate.Exceptions;
using VerticalSliceArchitectureTemplate.Extensions;
using VerticalSliceArchitectureTemplate.Features.BookFeature.CreateBook;
using VerticalSliceArchitectureTemplate.Repository;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));

builder.Services.AddSQLDatabaseConfiguration(builder.Configuration);

builder.Services.RegisterApiEndpointsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddHealthChecksConfiguration();

builder.Services.AddValidatorsFromAssembly(typeof(CreateBookValidator).Assembly);

builder.Services.AddHandlersFromAssembly(typeof(Program).Assembly);

builder.Services.AddExceptionHandler<CustomExceptionHandler>()
            .AddProblemDetails();


var app = builder.Build();

app.MapApiEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHealthChecks();


app.MapScalarApiReference(options =>
{
    options.WithTheme(ScalarTheme.DeepSpace);

    // ?? THIS enables Developer Tools (request + client code panel)
    options.WithDefaultHttpClient(
        ScalarTarget.CSharp,
        ScalarClient.HttpClient);

});

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.Run();

