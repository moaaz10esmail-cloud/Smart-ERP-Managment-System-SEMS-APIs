using SEMS.Core.Common;

namespace SEMS.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly SemsDbContext _db;
    public UnitOfWork(SemsDbContext db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}

