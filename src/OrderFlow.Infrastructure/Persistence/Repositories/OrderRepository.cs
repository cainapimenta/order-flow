
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Faz a persistência e as consultas dos pedidos.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderFlowDbContext _context;

    /// <summary>
    /// Cria o repositório com o contexto do banco.
    /// </summary>
    /// <param name="context">Contexto utilizado para acessar os pedidos.</param>
    public OrderRepository(OrderFlowDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adiciona um pedido para ser salvo no banco.
    /// </summary>
    /// <param name="order">Pedido que será adicionado.</param>
    public void Add(Order order)
    {
        _context.Orders.Add(order);
    }

    /// <summary>
    /// Busca um pedido pelo ID, incluindo seus itens.
    /// </summary>
    /// <param name="id">ID do pedido.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Pedido encontrado ou null.</returns>
    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(
                order => order.Id == id,
                cancellationToken);
    }

    /// <summary>
    /// Busca todos os pedidos, incluindo seus itens.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Lista de pedidos.</returns>
    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
