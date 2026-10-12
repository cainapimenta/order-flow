using Microsoft.EntityFrameworkCore;
using OrderFlow.Infrastructure.Messaging;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Worker.Services;

/// <summary>
/// Busca e publica as mensagens pendentes da Outbox.
/// </summary>
public sealed class OutboxPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqPublisher _publisher;
    private readonly ILogger<OutboxPublisherService> _logger;

    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    /// <summary>
    /// Cria o serviço de publicação da Outbox.
    /// </summary>
    /// <param name="scopeFactory">Responsável por criar escopos de serviços.</param>
    /// <param name="publisher">Publicador de mensagens no RabbitMQ.</param>
    /// <param name="logger">Responsável pelos logs do serviço.</param>
    public OutboxPublisherService(IServiceScopeFactory scopeFactory, RabbitMqPublisher publisher, ILogger<OutboxPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Executa a publicação das mensagens em segundo plano.
    /// </summary>
    /// <param name="stoppingToken">Token para encerrar o serviço.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Publisher started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ..
            }
            catch (Exception)
            {

                throw;
            }
        }
    }

    /// <summary>
    /// Busca e publica um lote de mensagens pendentes.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider
                .GetRequiredService<OrderFlowDbContext>();

        var messages = await context.OutboxMessages
            .Where(x => x.PublishedAt == null)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await _publisher.PublichAsync(message, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch(Exception ex)
            {
                message.RegisterFailure(ex.Message);

                await context.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(ex, "Failed to publish message {MessageId}.", message.Id);

                // Tenta novamente no próximo ciclo.
                break;
            }

            message.MarkAsPublished();

            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Outbox message {MessageId} published. OrderId: {OrderId}", message.Id, message.OrderId);
        }
    }
}