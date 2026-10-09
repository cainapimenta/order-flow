using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;

namespace OrderFlow.UnitTests.Domain.Entities;

/// <summary>
/// Testes das regras de negócio da entidade Order.
/// </summary>
public class OrderTests
{
    /// <summary>
    /// Verifica se um pedido é criado com os dados e status corretos.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateOrder_WithPendingStatus()
    {
        // Arrange & Act
        var order = new Order("João");
        
        //Assert
        Assert.Equal(0, order.Id);
        Assert.Equal("João", order.Customer);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Empty(order.Items);
    }

    /// <summary>
    /// Não permite criar pedidos sem um cliente válido.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrow_WhenCustomerIsInvalid(string? customer)
    {
        //Act
        var action = () => new Order(customer);

        //Assert
        Assert.Throws<ArgumentException>(action);
    }

    /// <summary>
    /// Verifica se o total do pedido é calculado corretamente.
    /// </summary>
    [Fact]
    public void AddItem_ShouldCalculateTotalAmount()
    {
        //Arrange
        var order = new Order("Maria");

        //Act
        order.AddItem("Teclado RedDragon", 2, 149.99m);
        order.AddItem("Mouse HyperX", 1, 80m);

        //Assert
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(379.98m, order.Amount);
    }

    /// <summary>
    /// Permite iniciar o processamento de pedidos que possuem itens.
    /// </summary>
    [Fact]
    public void MarkAsProcessing_ShouldChangeStatus_WhenOrderHasItems()
    {
        //Arrange
        var order = new Order("Carlos");
        order.AddItem("Monitor LG", 1, 1200m);

        //Act
        order.MarkAsProcessing();

        //Assert
        Assert.Equal(OrderStatus.Processing, order.Status);
    }

    /// <summary>
    /// Não permite processar um pedido vazio.
    /// </summary>
    [Fact]
    public void MarkAsProcessing_ShouldThrow_WhenOrderHasNoItems()
    {
        //Arrange
        var order = new Order("Ana");

        //Act
        var action = () => order.MarkAsProcessing();

        //Assert
        Assert.Throws<InvalidOperationException>(action);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    /// <summary>
    /// Não permite finalizar um pedido que ainda está pendente.
    /// </summary>
    [Fact]
    public void MarkAsFinished_ShouldThrow_WhenOrderIsPending()
    {
        //Arrange
        var order = new Order("Pedro");
        order.AddItem("Cadeira Gamer", 1, 500m);

        //Act
        var action = () => order.MarkAsFinished();

        //Assert
        Assert.Throws<InvalidOperationException>(action);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    /// <summary>
    /// Verifica se o pedido segue a sequência correta de status.
    /// </summary>
    [Fact]
    public void Order_ShouldFollowCorrectStatusSequence()
    {
        //Arrange
        var order = new Order("Lucas");
        order.AddItem("Placa mãe", 2, 400.99m);

        //Act & Assert
        Assert.Equal(OrderStatus.Pending, order.Status);

        order.MarkAsProcessing();
        Assert.Equal(OrderStatus.Processing, order.Status);

        order.MarkAsFinished();
        Assert.Equal(OrderStatus.Finished, order.Status);
    }

    /// <summary>
    /// Não permite adicionar itens após iniciar o processamento.
    /// </summary>
    [Fact]
    public void AddItem_ShouldThrow_WhenOrderIsProcessing()
    {
        //Arrange
        var order = new Order("Fernanda");
        order.AddItem("SSD Kingston", 1, 300m);
        order.MarkAsProcessing();

        //Act
        var action = () => order.AddItem("Memória RAM", 2, 150m);

        //Assert
        Assert.Throws<InvalidOperationException>(action);
        Assert.Single(order.Items);
    }

    /// <summary>
    /// Verifica se o item recebe o ID do pedido ao qual pertence.
    /// </summary>
    [Fact]
    public void AddItem_ShouldAssociateItemWithOrder()
    {
        //Arrange
        var order = new Order("Gabriel");

        //Act
        order.AddItem("Placa de vídeo", 1, 2500m);

        //Assert
        var item = Assert.Single(order.Items);
        Assert.Equal(order.Id, item.OrderId);
    }
}

