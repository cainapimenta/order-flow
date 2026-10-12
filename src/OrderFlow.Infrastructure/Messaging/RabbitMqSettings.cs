namespace OrderFlow.Infrastructure.Messaging;

/// <summary>
/// Configurações de conexão com RabbitMQ.
/// </summary>
public sealed class RabbitMqSettings
{
    /// <summary>
    /// Endereço do servidor RabbitMQ.
    /// </summary>
    public string HostName { get; set; } = "localhost";
    /// <summary>
    /// Porta utilizada na conexão.
    /// </summary>
    public int Port { get; set; } = 5672;
    /// <summary>
    /// Usuário utilizado na conexão.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>
    /// Senha utilizada na conexão.
    /// </summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// Nome da fila de pedidos criados.
    /// </summary>
    public string QueueName { get; set; } = "order-created";
}
