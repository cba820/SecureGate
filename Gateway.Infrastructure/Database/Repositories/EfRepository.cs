// Gateway.Infrastructure/Database/Repositories/EfRepository.cs
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Gateway.Infrastructure.Database.Context;

namespace Gateway.Infrastructure.Database.Repositories;

/// <summary>
/// Repositorio genérico basado en Ardalis.Specification.
/// Soporta queries con Specification, CRUD básico y async.
/// </summary>
public sealed class EfRepository<T> : RepositoryBase<T>, IReadRepositoryBase<T>, IRepositoryBase<T>
    where T : class {
    public EfRepository(GatewayDbContext dbContext) : base(dbContext) {
    }
}
