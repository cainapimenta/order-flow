using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderFlow.Infrastructure.Messaging;
using RabbitMQ.Client;

namespace OrderFlow.Api.HealthChecks;

/// <summary>
/// Verifica se o RabbitMQ está disponível.
/// </summary>
public sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly RabbitMqSettings _settings;

    /// <summary>
    /// Cria o health check com as configurações do RabbitMQ.
    /// </summary>
    /// <param name="settings">Configurações de conexão do RabbitMQ.</param>
    public RabbitMqHealthCheck(RabbitMqSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Verifica a conexão com o RabbitMQ.
    /// </summary>
    /// <param name="context">Informações da verificação.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Estado de saúde do RabbitMQ.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);

            return connection.IsOpen ? HealthCheckResult.Healthy("RabbitMQ is available.") : HealthCheckResult.Unhealthy("RabbitMQ is unavailable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy( "RabbitMQ connection failed.", ex);
        }
    }
}
