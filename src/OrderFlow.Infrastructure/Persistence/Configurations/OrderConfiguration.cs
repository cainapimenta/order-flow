using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura a tabela de pedidos.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{   
    /// <summary>
    /// Define como os pedidos serão mapeados no banco.
    /// </summary>
    /// <param name="builder">Configurações da entidade Order.</param>
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Customer)
            .HasColumnName("customer")
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Ignore(x => x.Amount);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configura o relacionamento com o histórico de status.
        builder.HasMany(order => order.StatusHistory)
            .WithOne()
            .HasForeignKey(history => history.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Permite que o EF Core acesse a coleção privada.
        builder.Navigation(order => order.StatusHistory)
            .HasField("_statusHistory")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Utiliza a versão do registro para detectar alterações concorrentes.
        builder.Property<uint>("xmin")
            .IsRowVersion();
    }
}
