using OrderFlow.Contracts.Events;

namespace OrderFlow.Application.Abstractions.Persistence;

/// <summary>
/// Registra eventos que precisam ser publicados.
/// </summary>
public interface IOutboxWriter
{
    /// <summary>
    /// Adiciona um evento de pedido criado à Outbox.
    /// </summary>
    /// <param name="message">Evento que será registrado.</param>
    void Add(OrderCreatedEvent message);
}
