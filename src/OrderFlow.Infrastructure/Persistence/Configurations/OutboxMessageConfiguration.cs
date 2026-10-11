using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configura a tabela de mensagens da Outbox.
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <summary>
    /// Define como as mensagens serão mapeadas no banco.
    /// </summary>
    /// <param name="builder">Configurações da entidade OutboxMessage.</param>
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
           .HasColumnName("id")
           .ValueGeneratedOnAdd();

        builder.Property(message => message.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(message => message.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(message => message.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(message => message.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(message => message.PublishedAt)
            .HasColumnName("published_at");

        builder.Property(message => message.Attempts)
            .HasColumnName("attempts")
            .IsRequired();

        builder.Property(message => message.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(2000);

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(message => message.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(message => new
        {
            message.PublishedAt,
            message.CreatedAt
        })
        .HasDatabaseName(
            "ix_outbox_messages_published_at_created_at");
    }
}
