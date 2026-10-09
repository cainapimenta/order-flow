namespace OrderFlow.Application.Orders;

/// <summary>
/// Dados de um item para criar um pedido.
/// </summary>
/// <param name="Product">Nome do produto.</param>
/// <param name="Quantity">Quantidade do produto.</param>
/// <param name="UnitPrice">Preço unitário do produto.</param>
public sealed record CreateOrderItemCommand(
    string Product,
    int Quantity,
    decimal UnitPrice
);
