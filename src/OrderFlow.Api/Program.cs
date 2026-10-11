using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Orders;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Persistence.Repositories;
using OrderFlow.Infrastructure.Persistence.Outbox;

var builder = WebApplication.CreateBuilder(args);

// Obtém a conexão com o PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not found");

// Registra o DbContext na injeção de dependências
builder.Services.AddDbContext<OrderFlowDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registra o repositório de pedidos.
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Registra o serviço responsável pela Outbox.
builder.Services.AddScoped<IOutboxWriter, OutboxWriter>();

// Utiliza o mesmo DbContext para controlar a gravação.
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<OrderFlowDbContext>());

// Registra o serviço responsável pelos pedidos.
builder.Services.AddScoped<OrderService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<OrderFlowDbContext>();

    await dbContext.Database.MigrateAsync();

    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
