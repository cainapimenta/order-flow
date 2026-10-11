using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Contracts.Events;
using OrderFlow.Infrastructure.Persistence.Entities;
using System.Text.Json;

namespace OrderFlow.Infrastructure.Persistence.Outbox;

/// <summary>
/// Registra mensagens pendentes no banco.
/// </summary>
public class OutboxWriter : IOutboxWriter
{
    private readonly OrderFlowDbContext _context;

    /// <summary>
    /// Cria o serviço de gravação da Outbox.
    /// </summary>
    /// <param name="context">Contexto do banco de dados.</param>
    public OutboxWriter(OrderFlowDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adiciona um evento de pedido criado à Outbox.
    /// </summary>
    /// <param name="message">Evento que será registrado.</param>
    public void Add(OrderCreatedEvent message)
    {
        var payload = JsonSerializer.Serialize(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var outboxMessage = new OutboxMessage(message.OrderId, OrderCreatedEvent.EventType, payload);

        _context.OutboxMessages.Add(outboxMessage);
    }
}
