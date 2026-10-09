using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Contracts.Orders.Requests;

/// <summary>
/// Dados necessários para criar um pedido.
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// Nome do cliente.
    /// </summary>
    [Required]
    [StringLength(150)]
    public string Customer { get; init; } = string.Empty;

    /// <summary>
    /// Produtos que fazem parte do pedido.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; init; } = [];
}
