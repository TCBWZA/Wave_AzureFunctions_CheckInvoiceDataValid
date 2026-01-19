using EfCoreLab.DTOs;
using EfCoreLab.Validators;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights telemetry
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Register FluentValidation validators
// These are used by CustomerFunctionsWithFluentValidation
builder.Services.AddScoped<IValidator<CreateCustomerDto>, CreateCustomerValidator>();
builder.Services.AddScoped<IValidator<UpdateCustomerDto>, UpdateCustomerValidator>();

// Alternative: Register all validators in the assembly automatically
// builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerValidator>();

builder.Build().Run();
