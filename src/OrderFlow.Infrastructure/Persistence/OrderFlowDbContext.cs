using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Application.Abstractions.Persistence;

namespace OrderFlow.Infrastructure.Persistence;

/// <summary>
/// Contexto do banco de dados da aplicação.
/// </summary>
public class OrderFlowDbContext : DbContext, IUnitOfWork
{
    /// <summary>
    /// Cria o contexto do banco de dados.
    /// </summary>
    /// <param name="options">Configurações utilizadas pelo contexto.</param>
    public OrderFlowDbContext(DbContextOptions<OrderFlowDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Pedidos cadastrados.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// Itens dos pedidos.
    /// </summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// Aplica as configurações das entidades no banco.
    /// </summary>
    /// <param name="modelBuilder">Responsável por configurar o modelo das entidades.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderFlowDbContext).Assembly);
    }
}
