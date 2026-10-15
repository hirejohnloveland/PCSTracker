using Ziosk.GoldenPattern.Api;
using Ziosk.GoldenPattern.Core;

var builder = WebApplication.CreateBuilder(args);

builder.AddCore();
builder.AddApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
