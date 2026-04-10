using Blockchain.Application.Service;
using Blockchain.Domain.Interfaces;
using Blockchain.Infrastructure.External;
using Blockchain.Infrastructure.Persistence;
using Blockchain.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SQLite DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=blockchain.db"));

// DI
builder.Services.AddScoped<IBlockchainRepository, BlockchainRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<BlockchainAppService>();

builder.Services.AddHttpClient<IBlockchainService, BlockchainService>();

// Swagger + Health
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
public partial class Program { }