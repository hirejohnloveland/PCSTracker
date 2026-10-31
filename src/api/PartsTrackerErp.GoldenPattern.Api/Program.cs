using PartsTrackerErp.GoldenPattern.Api;
using PartsTrackerErp.GoldenPattern.Core.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddCore();
builder.AddApi();

var app = builder.Build();

// First, so they wrap every stage after them.
app.UseExceptionHandler();
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
