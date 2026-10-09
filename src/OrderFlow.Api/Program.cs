using Microsoft.EntityFrameworkCore;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Obtém a conexão com o PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not found");

// Registra o DbContext na injeção de dependências
builder.Services.AddDbContext<OrderFlowDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registra o repositório de pedidos.
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Utiliza o mesmo DbContext para controlar a gravação.
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<OrderFlowDbContext>());

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
