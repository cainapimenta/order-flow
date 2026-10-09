namespace OrderFlow.Domain.Entities;

/// <summary>
/// Item de um pedido.
/// </summary>
public class OrderItem
{
    /// <summary>
    /// ID do item.
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// ID do pedido ao qual o item pertence.
    /// </summary>
    public int OrderId { get; private set; }
    /// <summary>
    /// Nome do produto.
    /// </summary>
    public string Product { get; private set; }
    /// <summary>
    /// Quantidade do produto.
    /// </summary>
    public int Quantity { get; private set; }
    /// <summary>
    /// Preço de cada unidade do produto.
    /// </summary>
    public decimal UnitPrice { get; private set; }
    /// <summary>
    /// Total do item (quantidade x preço unitário).
    /// </summary>
    public decimal SubTotal => Quantity * UnitPrice;

    // Necessário para o Entity Framework Core.
    private OrderItem()
    {
        Product = null;
    }

    /// <summary>
    /// Cria um novo item do pedido.
    /// </summary>
    /// <param name="product">Nome do produto.</param>
    /// <param name="quantity">Quantidade do produto.</param>
    /// <param name="unitPrice">Preço unitário do produto.</param>
    public OrderItem(string product, int quantity, decimal unitPrice)
    {
        if(string.IsNullOrWhiteSpace(product))
            throw new ArgumentException("Product is required..", nameof(product));

        if(quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if(unitPrice < 0)
            throw new ArgumentException("Unit price must be greater than zero.", nameof(unitPrice));


        Product = product.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
