using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Application.Orders;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Persistence.Repositories;
using OrderFlow.Infrastructure.Persistence.Outbox;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderFlow.Api.HealthChecks;
using OrderFlow.Infrastructure.Messaging;

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

// Obtém as configurações do RabbitMQ.
var rabbitMqSettings = builder.Configuration
    .GetSection("RabbitMq")
    .Get<RabbitMqSettings>() ?? throw new InvalidOperationException("RabbitMQ settings not found.");

builder.Services.AddSingleton(rabbitMqSettings);

// Registra os Health Checks.
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck<PostgreSqlHealthCheck>("postgres", tags: ["ready"], timeout: TimeSpan.FromSeconds(5))
    .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["ready"], timeout: TimeSpan.FromSeconds(5));

// Permite requisições do frontend React em desenvolvimento.
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDevelopment", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using var scope = app.Services.CreateScope();

var dbContext = scope.ServiceProvider
    .GetRequiredService<OrderFlowDbContext>();

await dbContext.Database.MigrateAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("ReactDevelopment");

app.UseAuthorization();

app.MapControllers();

// Verifica se a API está respondendo.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration =>
        registration.Tags.Contains("live")
});

// Verifica as dependências da aplicação.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration =>
        registration.Tags.Contains("ready")
});

app.Run();
