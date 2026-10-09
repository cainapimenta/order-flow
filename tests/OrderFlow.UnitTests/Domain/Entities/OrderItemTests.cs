using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Domain.Entities;

public class OrderItemTests
{
    /// <summary>
    /// Verifica se o item é criado com os dados corretos.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateItem_WithValidData()
    {

        // Arrange
        var product = "Notebook";
        var quantity = 2;
        var unitPrice = 3500m;

        // Act
        var item = new OrderItem(product, quantity, unitPrice);

        // Assert
        Assert.Equal(0, item.Id);
        Assert.Equal(0, item.OrderId);
        Assert.Equal(product, item.Product);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(unitPrice, item.UnitPrice);
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
        //Arrange & Act
        var action = () => new OrderItem(product, 2, 149);

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
        //Arrange & Act
        var action = () => new OrderItem("Teclado RedDragon", quantity, 149);

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
        //Arrange & Act
        var action = () => new OrderItem("Teclado RedDragon", 2, unitPrice);

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
        var item = new OrderItem("Teclado RedDragon", 2, 149.99m);

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
        //Arrange & Act
        var item = new OrderItem("  Teclado RedDragon  ", 2, 149.99m);

        //Assert
        Assert.Equal("Teclado RedDragon", item.Product);
    }
}
