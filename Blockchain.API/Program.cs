using Blockchain.API.Middleware;
using Blockchain.Application.Service;
using Blockchain.Domain.Interfaces;
using Blockchain.Infrastructure.Caching;
using Blockchain.Infrastructure.External;
using Blockchain.Infrastructure.Logging;
using Blockchain.Infrastructure.Persistence;
using Blockchain.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// SQLite DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=blockchain.db"));

// Dependency Injection
builder.Services.AddScoped<IBlockchainRepository, BlockchainRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<BlockchainAppService>();
builder.Services.AddScoped<IBlockchainLogger, BlockchainLogger>();

// Caching
builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();

// HttpClient for BlockCypher
builder.Services.AddHttpClient<IBlockchainService, BlockchainService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Blockchain API",
        Version = "v1",
        Description = "API for fetching and storing blockchain data from BlockCypher"
    });
});

// Health Checks
builder.Services.AddHealthChecks();

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Database migration
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blockchain API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }