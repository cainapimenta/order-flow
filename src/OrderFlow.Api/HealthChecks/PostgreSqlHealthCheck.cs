using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Api.HealthChecks;

/// <summary>
/// Verifica se o PostgreSQL está disponível.
/// </summary>
public sealed class PostgreSqlHealthCheck : IHealthCheck
{
    private readonly OrderFlowDbContext _context;

    /// <summary>
    /// Cria o health check com o contexto do banco.
    /// </summary>
    /// <param name="context">Contexto utilizado para verificar a conexão.</param>
    public PostgreSqlHealthCheck(OrderFlowDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Verifica a conexão com o PostgreSQL.
    /// </summary>
    /// <param name="context">Informações da verificação.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Estado de saúde do PostgreSQL.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database
                .CanConnectAsync(cancellationToken);

            return canConnect ? HealthCheckResult.Healthy("PostgreSQL is available.") : HealthCheckResult.Unhealthy("PostgreSQL is unavailable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection failed.", ex);
        }
    }
}
