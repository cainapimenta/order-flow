namespace OrderFlow.Application.Orders;

/// <summary>
/// Dados necessários para executar a criação de um pedido.
/// </summary>
/// <param name="Customer">Nome do cliente.</param>
/// <param name="Items">Itens que serão adicionados ao pedido.</param>
public sealed record CreateOrderCommand(
    string Customer,
    IReadOnlyList<CreateOrderItemCommand> Items
);
