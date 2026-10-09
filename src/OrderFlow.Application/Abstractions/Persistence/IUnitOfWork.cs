
namespace OrderFlow.Application.Abstractions.Persistence;

/// <summary>
/// Controla a gravação das alterações no banco.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Salva as alterações pendentes no banco.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Quantidade de entradas afetadas.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
