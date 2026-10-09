using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders;

/// <summary>
/// Coordena a criação e as consultas de pedidos.
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Cria o serviço com as dependências de persistência.
    /// </summary>
    /// <param name="orderRepository">Repositório de pedidos.</param>
    /// <param name="unitOfWork">Responsável por salvar as alterações.</param>
    public OrderService(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Cria e salva um novo pedido.
    /// </summary>
    /// <param name="command">Dados do pedido.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Pedido criado.</returns>
    public async Task<Order> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Items is null || command.Items.Count == 0)
            throw new ArgumentException(
                "An order must contain at least one item.");

        var order = new Order(command.Customer);

        foreach (var item in command.Items)
        {
            order.AddItem(
                item.Product,
                item.Quantity,
                item.UnitPrice);
        }

        _orderRepository.Add(order);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order;
    }

    /// <summary>
    /// Busca um pedido pelo ID.
    /// </summary>
    /// <param name="id">ID do pedido.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Pedido encontrado ou null.</returns>
    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Lista todos os pedidos cadastrados.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Lista de pedidos.</returns>
    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetAllAsync(cancellationToken);
    }
}
