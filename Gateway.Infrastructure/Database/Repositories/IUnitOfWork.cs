// Gateway.Infrastructure/Database/UnitOfWork/IUnitOfWork.cs
using Ardalis.Specification;

namespace Gateway.Infrastructure.Database.Repositories;

public interface IUnitOfWork : IDisposable {
    /// <summary>
    /// Obtiene (y cachea) un repositorio genérico para el tipo T.
    /// </summary>
    IRepositoryBase<T> Repository<T>() where T : class;

    /// <summary>
    /// Persiste cambios pendientes en el DbContext.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
