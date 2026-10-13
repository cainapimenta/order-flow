
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura a tabela de histórico de status.
/// </summary>
public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    /// <summary>
    /// Define como o histórico será salvo no banco.
    /// </summary>
    /// <param name="builder">Configurações da entidade OrderStatusHistory.</param>
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_history");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(history => history.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(history => history.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(history => history.ChangedAt)
            .HasColumnName("changed_at")
            .IsRequired();

        builder.HasIndex(history => new
        {
            history.OrderId,
            history.ChangedAt
        })
        .HasDatabaseName(
            "ix_order_status_history_order_id_changed_at");
    }
}
