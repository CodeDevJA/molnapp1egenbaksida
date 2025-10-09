using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ===== AZURE FUNCTIONS STARTUP CONFIGURATION =====
// This file configures the Function app when it starts
// Your existing code is perfect - no changes needed!

var builder = FunctionsApplication.CreateBuilder(args);

// Configure the Functions worker
builder.ConfigureFunctionsWebApplication();

// Add Application Insights for logging and monitoring
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Build and run the application
builder.Build().Run();
