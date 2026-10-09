namespace OrderFlow.Contracts.Orders.Responses;

/// <summary>
/// Dados de um pedido retornado pela API.
/// </summary>
public class OrderResponse
{
    /// <summary>
    /// ID do pedido.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Nome do cliente.
    /// </summary>
    public string Customer { get; init; } = string.Empty;

    /// <summary>
    /// Valor total do pedido.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Status atual do pedido.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Data de criação do pedido.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Itens do pedido.
    /// </summary>
    public IReadOnlyList<OrderItemResponse> Items { get; init; } = [];
}
