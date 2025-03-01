using Microsoft.EntityFrameworkCore;
using SeekCasinoIO.Application.Common.Interfaces.Repositories;
using SeekCasinoIO.Core.Entities.Base;
using System.Linq.Expressions;

namespace SeekCasinoIO.Infrastructure.Persistence.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
        int? pageSize = null, 
        int? pageNumber = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate, 
        int? pageSize = null,
        int? pageNumber = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.Where(predicate).AsNoTracking();

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null, 
        CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public void Add(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    public void AddRange(IEnumerable<TEntity> entities)
    {
        _dbSet.AddRange(entities);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public async Task<IReadOnlyList<TResult>> SelectAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        int? pageSize = null,
        int? pageNumber = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
        }

        return await query.Select(selector).ToListAsync(cancellationToken);
    }
}