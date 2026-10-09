namespace OrderFlow.Contracts.Orders.Responses;

/// <summary>
/// Dados de um item retornado pela API.
/// </summary>
public class OrderItemResponse
{
    /// <summary>
    /// ID do item.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// ID do pedido.
    /// </summary>
    public int OrderId { get; init; }

    /// <summary>
    /// Nome do produto.
    /// </summary>
    public string Product { get; init; } = string.Empty;

    /// <summary>
    /// Quantidade do produto.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Preço unitário do produto.
    /// </summary>
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// Valor total do item.
    /// </summary>
    public decimal SubTotal { get; init; }
}
