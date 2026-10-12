using OrderFlow.Infrastructure.Persistence.Entities;
using RabbitMQ.Client;
using System.Globalization;
using System.Text;

namespace OrderFlow.Infrastructure.Messaging;

/// <summary>
/// Publica mensagens no RabbitMQ.
/// </summary>
public sealed class RabbitMqPublisher : IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IChannel? _channel;

    /// <summary>
    /// Cria o publicador com as configurações do RabbitMQ.
    /// </summary>
    /// <param name="settings">Configurações de conexão e da fila.</param>
    public RabbitMqPublisher(RabbitMqSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Publica uma mensagem e aguarda a confirmação do RabbitMQ.
    /// </summary>
    /// <param name="message">Mensagem da Outbox.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    public async Task PublichAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            await EnsureConnectedAsync(cancellationToken);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                Persistent = true,
                MessageId = message.Id.ToString(CultureInfo.InvariantCulture),
                CorrelationId = message.OrderId.ToString(CultureInfo.InvariantCulture),
                Type = message.EventType
            };

            var body = Encoding.UTF8.GetBytes(message.Payload);

            await _channel!.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _settings.QueueName,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken
            );
        }
        catch
        {
            await ResetConnectionAsync();

            throw;
        }
    }

    /// <summary>
    /// Garante que a conexão e o canal estejam disponíveis.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            return;

        await ResetConnectionAsync();

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            ClientProvidedName = "order-flow-outbox-publisher",
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);

        var channelOptions = new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true);
        _channel = await _connection.CreateChannelAsync(channelOptions, cancellationToken);

       await _channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Fecha a conexão atual para permitir uma nova tentativa.
    /// </summary>
    private async Task ResetConnectionAsync()
    {
        if(_channel is not null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }

        if(_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    /// <summary>
    /// Libera a conexão quando o publicador for encerrado.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await ResetConnectionAsync();
    }
}
