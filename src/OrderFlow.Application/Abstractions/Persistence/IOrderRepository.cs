using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Abstractions.Persistence;

/// <summary>
/// Define as operações de persistência dos pedidos.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Adiciona um pedido para ser salvo no banco.
    /// </summary>
    /// <param name="order">Pedido que será adicionado.</param>
    void Add(Order order);

    /// <summary>
    /// Busca um pedido pelo ID.
    /// </summary>
    /// <param name="id">ID do pedido.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Pedido encontrado ou null.</returns>
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca todos os pedidos cadastrados.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Lista de pedidos.</returns>
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);

}
