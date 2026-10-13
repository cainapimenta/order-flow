using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Infrastructure.Persistence.Entities;

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
    /// Histórico de mudanças de status dos pedidos.
    /// </summary>
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();

    /// <summary>
    /// Mensagens registradas para publicação.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Executa operações dentro de uma transação.
    /// </summary>
    /// <param name="operation">Operações que serão executadas.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation, nameof(operation));

        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);

        await operation(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

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
