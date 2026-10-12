using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Orders;

/// <summary>
/// Coordena o processamento dos pedidos.
/// </summary>
public sealed class OrderProcessingService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Cria o serviço responsável pelo processamento dos pedidos.
    /// </summary>
    /// <param name="orderRepository">Repositório de pedidos.</param>
    /// <param name="unitOfWork">Responsável por salvar as alterações.</param>
    public OrderProcessingService(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Processa um pedido e atualiza seu status até a finalização.
    /// </summary>
    /// <param name="orderId">ID do pedido.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    public async Task ProcessAsync(int orderId, CancellationToken cancellationToken = default)
    {
        if (orderId <= 0)
            throw new ArgumentOutOfRangeException(nameof(orderId));

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
            throw new KeyNotFoundException($"Order {orderId} not found.");

        // Um pedido finalizado não precisa ser processado novamente.
        if (order.Status == OrderStatus.Finished)
            return;

        // Inicia o processamento apenas se estiver pendente.
        if (order.Status == OrderStatus.Pending)
        {
            order.MarkAsProcessing();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else if (order.Status != OrderStatus.Processing)
        {
            throw new InvalidOperationException($"Invalid order status: {order.Status}.");
        }

        // Simula o tempo de processamento exigido pelo desafio.
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        order.MarkAsFinished();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
