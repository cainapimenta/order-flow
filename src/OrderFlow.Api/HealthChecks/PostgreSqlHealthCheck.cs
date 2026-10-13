
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Api.HealthChecks;

/// <summary>
/// Verifica a disponibilidade do PostgreSQL.
/// </summary>
public sealed class PostgreSqlHealthCheck : IHealthCheck
{
    private readonly OrderFlowDbContext _context;
    private readonly ILogger<PostgreSqlHealthCheck> _logger;

    /// <summary>
    /// Inicializa o health check do PostgreSQL.
    /// </summary>
    /// <param name="context">Contexto do banco de dados.</param>
    /// <param name="logger">Serviço de registro de logs.</param>
    public PostgreSqlHealthCheck(
        OrderFlowDbContext context,
        ILogger<PostgreSqlHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Verifica a conexão com o PostgreSQL.
    /// </summary>
    /// <param name="context">Contexto da verificação.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado da verificação.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.OpenConnectionAsync(
                cancellationToken);

            await _context.Database.CloseConnectionAsync();

            return HealthCheckResult.Healthy("PostgreSQL is available.");
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to PostgreSQL.");

            return HealthCheckResult.Unhealthy("PostgreSQL connection failed.", ex);
        }
    }
}
