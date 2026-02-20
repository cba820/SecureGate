// Gateway.Infrastructure/Database/UnitOfWork/UnitOfWork.cs
using System.Collections.Concurrent;
using Ardalis.Specification;
using Gateway.Infrastructure.Database.Context;

namespace Gateway.Infrastructure.Database.Repositories;

public sealed class UnitOfWork : IUnitOfWork {
    private readonly GatewayDbContext _dbContext;

    // Cache por tipo de entidad: typeof(T) -> IRepositoryBase<T>
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(GatewayDbContext dbContext) {
        _dbContext = dbContext;
    }

    public IRepositoryBase<T> Repository<T>() where T : class {
        var type = typeof(T);

        // Si ya existe lo devuelve; si no, lo crea y lo cachea.
        var repo = _repositories.GetOrAdd(type, _ => {
            // EfRepository<T> : RepositoryBase<T>, IRepositoryBase<T>
            return new EfRepository<T>(_dbContext);
        });

        return (IRepositoryBase<T>)repo;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public void Dispose()
        => _dbContext.Dispose();
}
