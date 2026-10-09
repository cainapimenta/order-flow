using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Orders;
using OrderFlow.Contracts.Orders.Requests;
using OrderFlow.Contracts.Orders.Responses;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Api.Controllers;

/// <summary>
/// Endpoints para criar e consultar pedidos.
/// </summary>
[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    /// <summary>
    /// Cria o controller com o serviço de pedidos.
    /// </summary>
    /// <param name="orderService">Serviço responsável pelos pedidos.</param>
    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Converte um pedido para o formato retornado pela API.
    /// </summary>
    /// <param name="order">Pedido que será convertido.</param>
    /// <returns>Dados do pedido para a resposta.</returns>
    private static OrderResponse ToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            Customer = order.Customer,
            Amount = order.Amount,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            Items = order.Items
                .Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    OrderId = item.OrderId,
                    Product = item.Product,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SubTotal = item.SubTotal
                })
                .ToList()
        };
    }

    /// <summary>
    /// Busca um pedido pelo ID.
    /// </summary>
    /// <param name="id">ID do pedido.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Pedido encontrado.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(
            id,
            cancellationToken);

        if (order is null)
            return NotFound();

        return Ok(ToResponse(order));
    }

    /// <summary>
    /// Lista todos os pedidos.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Lista de pedidos cadastrados.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllAsync(cancellationToken);

        var response = orders
            .Select(ToResponse)
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Cria um novo pedido.
    /// </summary>
    /// <param name="request">Dados do novo pedido.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Pedido criado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.Customer,
            request.Items
                .Select(item => new CreateOrderItemCommand(
                    item.Product,
                    item.Quantity,
                    item.UnitPrice))
                .ToList());

        try
        {
            var order = await _orderService.CreateAsync(
                command,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = order.Id },
                ToResponse(order));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Pedido inválido",
                Detail = ex.Message
            });
        }
    }
} 
