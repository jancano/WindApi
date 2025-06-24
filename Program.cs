using WindApi.Interfaces;
using WindApi.Models;
using WindApi.Services;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
services.AddOpenApi();
services.AddControllers();
services.AddTransient<IWeatherService, DefaultWeatherService>();
services.AddSingleton(new OpenWeatherConfig(){ApiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY") ?? ""});

services.AddLogging();
services.AddHttpLogging();
var app = builder.Build();

app.UseHttpLogging();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();

