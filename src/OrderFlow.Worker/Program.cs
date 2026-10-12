
using Microsoft.EntityFrameworkCore;
using OrderFlow.Infrastructure.Messaging;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Obtém a conexão com o PostgreSQL.
var connectionStriing = builder.Configuration
        .GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Registra o contexto do banco de dados.
builder.Services.AddDbContext<OrderFlowDbContext>(options =>
    options.UseNpgsql(connectionStriing));

// Obtém as configurações do RabbitMQ.
var rabbitMqSettings = new RabbitMqSettings
{
    HostName = builder.Configuration["RabbitMq:HostName"] ?? "localhost",
    Port = builder.Configuration.GetValue("RabbitMq:Port", 5672),
    UserName = builder.Configuration["RabbitMq:UserName"] ?? throw new InvalidOperationException(
            "RabbitMQ username not found."),
    Password = builder.Configuration["RabbitMq:Password"] ?? throw new InvalidOperationException(
            "RabbitMQ password not found."),
    QueueName = builder.Configuration["RabbitMq:QueueName"] ?? "order-created"
};

// Registra as configurações e o publicador do RabbitMQ.
builder.Services.AddSingleton(rabbitMqSettings);
builder.Services.AddSingleton<RabbitMqPublisher>();

// Registra o serviço que publica as mensagens da Outbox.
builder.Services.AddHostedService<OutboxPublisherService>();

var host = builder.Build();
await host.RunAsync();