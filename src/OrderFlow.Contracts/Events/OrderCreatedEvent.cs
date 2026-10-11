namespace OrderFlow.Contracts.Events;

/// <summary>
/// Evento gerado quando um pedido é criado.
/// </summary>
/// <param name="OrderId">ID do pedido criado.</param>
public sealed record OrderCreatedEvent(int OrderId)
{
    /// <summary>
    /// Nome utilizado para identificar o evento.
    /// </summary>
    public const string EventType = "OrderCreated";
}
