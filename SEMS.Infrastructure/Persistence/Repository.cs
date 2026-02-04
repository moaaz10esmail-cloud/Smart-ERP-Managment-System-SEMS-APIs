using Microsoft.EntityFrameworkCore;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;

namespace SEMS.Infrastructure.Persistence;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly SemsDbContext _db;
    private readonly DbSet<T> _set;
    public Repository(SemsDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _set.FindAsync([id], cancellationToken);
    }
    public async Task<IReadOnlyList<T>> ListAsync(System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _set;
        if (predicate != null) query = query.Where(predicate);
        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }
    public async Task<PagedResult<T>> ListPagedAsync(PagedQuery pagedQuery, System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _set;
        
        if (predicate != null) 
            query = query.Where(predicate);

        // Filter by SearchTerm if entity has a Name property (Generic heuristic)
        // Note: In a real app, this should be handled by a specific Specification or Filter strategy
        // But for "Unified" demonstration, we'll keep it simple or rely on the predicate passed in.
        // If we want generic search, we'd need to know which fields to search.
        // For now, let's assume filtering happens via predicate or we skip generic search here.
        
        var totalCount = await query.CountAsync(cancellationToken);

        query = query.ApplySorting(pagedQuery.SortBy, pagedQuery.SortDirection);
        query = query.ApplyPaging(pagedQuery);

        var items = await query.AsNoTracking().ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, pagedQuery.Page, pagedQuery.PageSize);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _set.AddAsync(entity, cancellationToken);
    }
    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _set.Update(entity);
        return Task.CompletedTask;
    }
    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _set.Remove(entity);
        return Task.CompletedTask;
    }
}

