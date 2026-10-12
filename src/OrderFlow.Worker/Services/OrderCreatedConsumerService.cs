using OrderFlow.Application.Orders;
using OrderFlow.Contracts.Events;
using OrderFlow.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Globalization;
using System.Text.Json;

namespace OrderFlow.Worker.Services;

/// <summary>
/// Consome os eventos de pedidos criados no RabbitMQ.
/// </summary>
public sealed class OrderCreatedConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<OrderCreatedConsumerService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Cria o serviço responsável por consumir os pedidos.
    /// </summary>
    /// <param name="scopeFactory">Responsável por criar escopos de serviços.</param>
    /// <param name="settings">Configurações do RabbitMQ.</param>
    /// <param name="logger">Responsável pelos logs do serviço.</param>
    public OrderCreatedConsumerService(IServiceScopeFactory scopeFactory, RabbitMqSettings settings, ILogger<OrderCreatedConsumerService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Mantém o consumidor em execução e tenta reconectar em caso de falha.
    /// </summary>
    /// <param name="stoppingToken">Token para encerrar o serviço.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ consumer disconnected.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Abre a conexão e começa a receber mensagens.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            ClientProvidedName = "order-flow-order-consumer",
            // Vamos controlar as reconexões neste serviço.
            AutomaticRecoveryEnabled = false,
            ConsumerDispatchConcurrency = 1
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        // Recebe apenas uma mensagem não confirmada por vez.
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, delivery) =>
        {
            await HandleMessageAsync(
                channel,
                delivery,
                cancellationToken);
        };

        await channel.BasicConsumeAsync(
            queue: _settings.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Order consumer started. Queue: {QueueName}", _settings.QueueName);

        // Mantém a conexão ativa enquanto estiver disponível.
        while (!cancellationToken.IsCancellationRequested && connection.IsOpen && channel.IsOpen)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        if (!cancellationToken.IsCancellationRequested)
            throw new InvalidOperationException("RabbitMQ connection or channel was closed.");
    }

    /// <summary>
    /// Recebe uma mensagem, processa o pedido e confirma o consumo.
    /// </summary>
    /// <param name="channel">Canal utilizado para consumir a mensagem.</param>
    /// <param name="delivery">Mensagem recebida do RabbitMQ.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    private async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken cancellationToken)
    {
        OrderCreatedEvent? orderCreated;

        try
        {
            // Copia e interpreta o conteúdo JSON recebido.
            orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(delivery.Body.ToArray(), JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON received from RabbitMQ.");

            await channel.BasicRejectAsync(delivery.DeliveryTag, requeue: false, cancellationToken: cancellationToken);

            return;
        }

        // Confere o tipo de evento e o ID de correlação.
        if (orderCreated is null || orderCreated.OrderId <= 0 || delivery.BasicProperties.Type != OrderCreatedEvent.EventType ||
            delivery.BasicProperties.CorrelationId != orderCreated.OrderId.ToString(CultureInfo.InvariantCulture))
        {
            _logger.LogWarning("Invalid OrderCreated message received.");

            await channel.BasicRejectAsync(delivery.DeliveryTag, requeue: false, cancellationToken: cancellationToken);

            return;
        }

        try
        {
            _logger.LogInformation("Processing OrderId: {OrderId}", orderCreated.OrderId);

            await using var scope = _scopeFactory.CreateAsyncScope();

            var processingService = scope.ServiceProvider
                .GetRequiredService<OrderProcessingService>();

            await processingService.ProcessAsync(orderCreated.OrderId, cancellationToken);

            // Confirma somente depois de salvar o processamento.
            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false,  cancellationToken: cancellationToken);

            _logger.LogInformation("Order {OrderId} processed successfully.", orderCreated.OrderId);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // A mensagem permanece sem ACK e será reentregue
            // quando a conexão for encerrada.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderId: {OrderId}", orderCreated.OrderId);

            try
            {
                // Evita tentativas imediatas em sequência.
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                if (channel.IsOpen)
                {
                    await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                // O RabbitMQ poderá reentregar a mensagem.
            }
        }
    }
}
