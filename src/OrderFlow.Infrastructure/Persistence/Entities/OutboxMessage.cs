namespace OrderFlow.Infrastructure.Persistence.Entities;

/// <summary>
/// Mensagem que aguarda publicação no RabbitMQ.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// ID da mensagem.
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// ID do pedido relacionado à mensagem.
    /// </summary>
    public int OrderId { get; private set; }
    /// <summary>
    /// Tipo do evento.
    /// </summary>
    public string EventType { get; private set; } = null!;
    /// <summary>
    /// Conteúdo da mensagem em JSON.
    /// </summary>
    public string Payload { get; private set; } = null!;
    /// <summary>
    /// Data em que a mensagem foi criada.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// Data em que a publicação foi confirmada.
    /// </summary>
    public DateTime? PublishedAt { get; private set; }
    /// <summary>
    /// Quantidade de tentativas de publicação que falharam.
    /// </summary>
    public int Attempts { get; private set; }
    /// <summary>
    /// Último erro de publicação, se houver.
    /// </summary>
    public string? LastError { get; private set; }

    private OutboxMessage()
    {
        
    }

    /// <summary>
    /// Cria uma mensagem pendente de publicação.
    /// </summary>
    /// <param name="orderId">ID do pedido.</param>
    /// <param name="eventType">Tipo do evento.</param>
    /// <param name="payload">Conteúdo da mensagem em JSON.</param>
    public OutboxMessage(int orderId, string eventType, string payload)
    {
        if(orderId <= 0)
            throw new ArgumentOutOfRangeException(nameof(orderId));

        if(string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required.", nameof(eventType));

        if(string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload is required.", nameof(payload));

        OrderId = orderId;
        EventType = eventType;
        Payload = payload;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca a mensagem como publicada.
    /// </summary>
    public void MarkAsPublished()
    {
        PublishedAt = DateTime.UtcNow;
        LastError = null;
    }

    /// <summary>
    /// Registra uma falha na publicação.
    /// </summary>
    /// <param name="error">Mensagem do erro ocorrido.</param>
    public void RegisterFailure(string error)
    {
        Attempts++;
        LastError = error.Length > 200 ? error[..200] : error;
    }
}
