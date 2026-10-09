using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura a tabela de itens dos pedidos.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <summary>
    /// Define como os itens serão mapeados no banco.
    /// </summary>
    /// <param name="builder">Configurações da entidade OrderItem.</param>
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.Product)
            .HasColumnName("product")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(x => x.UnitPrice)
            .HasColumnName("unit_price")
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Ignore(x => x.SubTotal);
    }
}
