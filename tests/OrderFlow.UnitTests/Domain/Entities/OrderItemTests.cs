using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Domain.Entities;

public class OrderItemTests
{
    /// <summary>
    /// Verifica se o item é criado com os dados corretos.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateItem_WhithValidData()
    {

        //Arrange
        var orderId = Guid.NewGuid();

        //Act
        var item = new OrderItem(orderId, "Teclado RedDragon", 2, 149.99m);

        //Assert
        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(orderId, item.OrderId);
        Assert.Equal("Teclado RedDragon", item.Product);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(149.99m, item.UnitPrice);
    }

    /// <summary>
    /// Não permite criar itens sem um nome de produto válido.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrow_WhenProductIsInvalid(string? product)
    {
        //Arrange
        var orderId = Guid.NewGuid();

        //Act
        var action = () => new OrderItem(orderId, product, 2, 149);

        //Assert
        Assert.Throws<ArgumentException>(action);
    }

    /// <summary>
    /// Não permite criar itens com quantidade inválida.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Constructor_ShouldThrow_WhenQuantityIsInvalid(int quantity)
    {
        //Arrange
        var orderId = Guid.NewGuid();

        //Act
        var action = () => new OrderItem(orderId, "Teclado RedDragon", quantity, 149);

        //Assert
        Assert.Throws<ArgumentException>(action);
    }

    /// <summary>
    /// Não permite criar itens com preço unitário inválido.
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Constructor_ShouldThrow_WhenUnitPriceIsInvalid(decimal unitPrice)
    {
        //Arrange
        var orderId = Guid.NewGuid();

        //Act
        var action = () => new OrderItem(orderId, "Teclado RedDragon", 2, unitPrice);

        //Assert
        Assert.Throws<ArgumentException>(action);
    }

    /// <summary>
    /// Calcula o subtotal com base na quantidade e no preço unitário.
    /// </summary>
    [Fact]
    public void SubTotal_ShouldCalculateCorrectly()
    {
        //Arrange
        var orderId = Guid.NewGuid();
        var item = new OrderItem(orderId, "Teclado RedDragon", 2, 149.99m);

        //Act
        var subTotal = item.SubTotal;

        //Assert
        Assert.Equal(299.98m, subTotal);
    }

    /// <summary>
    /// Remove os espaços extras do nome do produto.
    /// </summary>
    [Fact]
    public void Constructor_ShouldTrimProductName()
    {
        //Arrange
        var orderId = Guid.NewGuid();

        //Act
        var item = new OrderItem(orderId, "  Teclado RedDragon  ", 2, 149.99m);

        //Assert
        Assert.Equal("Teclado RedDragon", item.Product);
    }
}
