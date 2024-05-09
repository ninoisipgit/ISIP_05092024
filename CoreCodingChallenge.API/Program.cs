using CoreCodingChallenge.API.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Redis cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Configure Redis server address and port
    options.InstanceName = "MyInstance"; // Unique instance name
});

// Add session services
builder.Services.AddSession();

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable session middleware
app.UseSession();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use API key authentication middleware
app.UseMiddleware<ApiKeyMiddleware>();

// Authorization should come after authentication middleware
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();
