using PartsTrackerErp.GoldenPattern.Api;
using PartsTrackerErp.GoldenPattern.Api.ErrorHandling;
using PartsTrackerErp.GoldenPattern.Core.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddCore();
builder.AddApi();

var app = builder.Build();

// First, so it wraps every stage after it.
app.UseMiddleware<GlobalExceptionHandler>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
