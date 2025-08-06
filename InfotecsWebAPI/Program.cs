using Microsoft.EntityFrameworkCore;
using InfotecsWebAPI.Data;
using InfotecsWebAPI.Services;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register ActivitySource as singleton
builder.Services.AddSingleton(new ActivitySource("InfotecsWebAPI"));

// Add Entity Framework and PostgreSQL
builder.Services.AddDbContext<TimescaleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add application services
builder.Services.AddScoped<ICsvProcessingService, CsvProcessingService>();

// Add controllers
builder.Services.AddControllers();

// Configure routing
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true; 
});

// Configure OpenTelemetry
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
});

var otel = builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => 
        resource.AddService(
            serviceName: "InfotecsWebAPI",
            serviceVersion: "1.0.0"))
    .WithTracing(tracingBuilder =>
    {
        tracingBuilder
            .AddSource("InfotecsWebAPI") // Add our custom activity source
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequest = (activity, httpRequest) =>
                {
                    activity.SetTag("http.request.body.size", httpRequest.ContentLength);
                };
                options.EnrichWithHttpResponse = (activity, httpResponse) =>
                {
                    activity.SetTag("http.response.body.size", httpResponse.ContentLength);
                };
            })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                {
                    activity.SetTag("http.request.method", httpRequestMessage.Method.ToString());
                };
                options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                {
                    activity.SetTag("http.response.status_code", (int)httpResponseMessage.StatusCode);
                };
            })
            .AddEntityFrameworkCoreInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
                options.SetDbStatementForStoredProcedure = true;
            })
            .AddConsoleExporter();
    })
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();
    });

var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (!string.IsNullOrEmpty(otlpEndpoint))
    otel.UseOtlpExporter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "Infotecs Web API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<TimescaleDbContext>();
await dbContext.Database.MigrateAsync();

app.Run();