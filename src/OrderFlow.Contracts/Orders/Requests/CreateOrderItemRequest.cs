using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Contracts.Orders.Requests;

/// <summary>
/// Dados de um item na criação do pedido.
/// </summary>
public class CreateOrderItemRequest
{
    /// <summary>
    /// Nome do produto.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Product { get; init; } = string.Empty;

    /// <summary>
    /// Quantidade do produto.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }

    /// <summary>
    /// Preço de cada unidade.
    /// </summary>
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal UnitPrice { get; init; }
}
