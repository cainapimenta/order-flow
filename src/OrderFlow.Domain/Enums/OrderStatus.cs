namespace OrderFlow.Domain.Enums;

/// <summary>
/// Status possíveis de um pedido.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Aguardando processamento.
    /// </summary>
    Pending = 1,
    /// <summary>
    /// Em processamento.
    /// </summary>
    Processing = 2,
    /// <summary>
    /// Processamento concluído.
    /// </summary>
    Finished = 3
}
