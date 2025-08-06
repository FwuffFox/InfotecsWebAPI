using Microsoft.EntityFrameworkCore;
using InfotecsWebAPI.Data;
using InfotecsWebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Entity Framework and PostgreSQL
builder.Services.AddDbContext<TimescaleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add application services
builder.Services.AddScoped<ICsvProcessingService, CsvProcessingService>();

// Add controllers
builder.Services.AddControllers();

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