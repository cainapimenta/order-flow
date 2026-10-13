
namespace OrderFlow.Contracts.Orders.Responses;

/// <summary>
/// Dados de uma mudança de status do pedido.
/// </summary>
public class OrderStatusHistoryResponse
{
    /// <summary>
    /// Status registrado.
    /// </summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>
    /// Data e hora da mudança de status.
    /// </summary>
    public DateTime ChangedAt { get; init; }
}
