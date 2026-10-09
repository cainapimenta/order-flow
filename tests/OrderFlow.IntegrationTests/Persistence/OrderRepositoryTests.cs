
using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Persistence.Repositories;
using Testcontainers.PostgreSql;

namespace OrderFlow.IntegrationTests.Persistence;

public class OrderRepositoryTests
{
    /// <summary>
    /// Verifica se um pedido e seus itens são salvos corretamente.
    /// </summary>
    [Fact]
    public async Task Add_ShouldPersistOrderAndItems()
    {
        // Arrange
        await using var postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .Build();

        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<OrderFlowDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;

        int orderId;

        await using (var context = new OrderFlowDbContext(options))
        {
            await context.Database.MigrateAsync();

            var repository = new OrderRepository(context);

            var order = new Order("João");

            order.AddItem("Teclado", 2, 150m);
            order.AddItem("Mouse", 1, 80m);

            // Act
            repository.Add(order);

            await context.SaveChangesAsync();

            orderId = order.Id;
        }

        // Assert
        Assert.True(orderId > 0);

        await using (var context = new OrderFlowDbContext(options))
        {
            var repository = new OrderRepository(context);

            var savedOrder = await repository.GetByIdAsync(orderId);

            Assert.NotNull(savedOrder);
            Assert.Equal("João", savedOrder.Customer);
            Assert.Equal(2, savedOrder.Items.Count);
            Assert.Equal(380m, savedOrder.Amount);

            Assert.All(savedOrder.Items, item =>
                Assert.Equal(savedOrder.Id, item.OrderId));
        }
    }
}