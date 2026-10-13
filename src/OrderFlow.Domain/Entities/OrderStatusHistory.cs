using OrderFlow.Domain.Enums;

namespace OrderFlow.Domain.Entities;

/// <summary>
/// Guarda uma mudança de status do pedido.
/// </summary>
public class OrderStatusHistory
{
    /// <summary>
    /// ID do registro.
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// ID do pedido.
    /// </summary>
    public int OrderId { get; private set; }
    /// <summary>
    /// Status registrado.
    /// </summary>
    public OrderStatus Status { get; private set; }
    /// <summary>
    /// Data e hora da mudança de status, em UTC.
    /// </summary>
    public DateTime ChangedAt { get; private set; }

    private OrderStatusHistory()
    {
    }

    /// <summary>
    /// Cria um registro de mudança de status.
    /// </summary>
    /// <param name="status">Novo status do pedido.</param>
    /// <param name="changedAt">Data e hora da mudança.</param>
    internal OrderStatusHistory(OrderStatus status, DateTime changedAt)
    {
        if (!Enum.IsDefined(status))
            throw new ArgumentOutOfRangeException(nameof(status));

        Status = status;
        ChangedAt = changedAt;
    }
}
