using OrderFlow.Domain.Enums;

namespace OrderFlow.Domain.Entities;

/// <summary>
/// Pedido com seus itens e status.
/// </summary>
public class Order
{
    private readonly List<OrderItem> _items = [];

    /// <summary>
    /// ID do pedido.
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// Nome do cliente.
    /// </summary>
    public string Customer { get; private set; }
    /// <summary>
    /// Status atual do pedido.
    /// </summary>
    public OrderStatus Status { get; private set; }
    /// <summary>
    /// Data em que o pedido foi criado.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// Itens que fazem parte do pedido.
    /// </summary>
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    /// <summary>
    /// Soma dos valores de todos os itens.
    /// </summary>
    public decimal Amount => _items.Sum(item => item.SubTotal);

    private Order()
    {
        Customer = null;
        _items = [];
    }

    /// <summary>
    /// Cria um novo pedido com status pendente.
    /// </summary>
    /// <param name="customer">Nome do cliente.</param>
    public Order(string customer)
    {
        if(string.IsNullOrWhiteSpace(customer))
            throw new ArgumentException("Customer is required.", nameof(customer));

        Customer = customer.Trim();
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        _items = [];
    }

    /// <summary>
    /// Adiciona um item ao pedido enquanto ele está pendente.
    /// </summary>
    public void AddItem(string product, int quantity, decimal unitPrice)
    {
        if(Status != OrderStatus.Pending)
            throw new InvalidOperationException("Items cannot be added after processing starts.");

        var item = new OrderItem(product, quantity, unitPrice);

        _items.Add(item);
    }

    /// <summary>
    /// Muda o status do pedido para Processando.
    /// </summary>
    public void MarkAsProcessing()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("OOnly pending orders can start processing.");

        if(_items.Count == 0)
            throw new InvalidOperationException("An order must contain at least one item.");

        Status = OrderStatus.Processing;
    }

    /// <summary>
    /// Muda o status do pedido para Finalizado.
    /// </summary>
    public void MarkAsFinished()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Only processing orders can be finished.");

        Status = OrderStatus.Finished;
    }   
}
