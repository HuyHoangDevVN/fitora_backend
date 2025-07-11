using System.Linq.Expressions;
using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.Pagination.Cursor;
using BuildingBlocks.RepositoryBase.EntityFramework;
using InteractService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InteractService.Infrastructure;

public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity>? _dbSet;

    public RepositoryBase(ApplicationDbContext dbContext)
    {
        _context = dbContext;
        _dbSet = _context.Set<TEntity>();
    }

    private static readonly Func<DbContext, Task<List<TEntity>>> GetAllCompiledQuery =
        EF.CompileAsyncQuery<DbContext, List<TEntity>>(context => context.Set<TEntity>()!.ToList());

    private static readonly Func<DbContext, Expression<Func<TEntity, bool>>, Task<List<TEntity>>> FindCompiledQuery =
        EF.CompileAsyncQuery<DbContext, Expression<Func<TEntity, bool>>, List<TEntity>>(
            (context, expression) => context.Set<TEntity>().Where(expression).ToList()
        );

    private static readonly Func<DbContext, Func<TEntity, bool>, Task<TEntity>> GetCompiledQuery =
        EF.CompileAsyncQuery<DbContext, Func<TEntity, bool>, TEntity>(
            (context, func) => context.Set<TEntity>().FirstOrDefault(func)!
        );

    private static readonly Func<DbContext, string, object, Task<TEntity>> GetByFieldCompiledQuery =
        EF.CompileAsyncQuery<DbContext, string, object, TEntity>(
            (context, fieldName, value) => context.Set<TEntity>()
                .FirstOrDefault(e => EF.Property<object>(e, fieldName).Equals(value))!
        );

    public IQueryable<TEntity> Query()
    {
        return _dbSet.AsQueryable();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet!.Where(expression).ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> func,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet!.FirstOrDefaultAsync(func, cancellationToken);
        return entity!;
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet!.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Expression<Func<TEntity, bool>> func, object payload,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet!.FirstOrDefaultAsync(func, cancellationToken) ??
                     throw new NotFoundException($"{typeof(TEntity).Name} not found");
        _context.Entry(entity).CurrentValues.SetValues(payload);
    }

    public async Task DeleteAsync(Expression<Func<TEntity, bool>> func, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet!.FirstOrDefaultAsync(func, cancellationToken) ??
                     throw new NotFoundException($"{typeof(TEntity).Name} not found");
        _dbSet!.Remove(entity);
    }

    public async Task DeleteRangeAsync(Expression<Func<TEntity, bool>> func,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet!.Where(func).ToListAsync(cancellationToken);
        if (entities.Any())
        {
            _dbSet!.RemoveRange(entities);
        }
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet!.AddRangeAsync(entities, cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        _dbSet!.UpdateRange(entities);
    }

    public async Task<IEnumerable<TResult>> GetSelectAsync<TResult>(Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? conditions, CancellationToken cancellationToken = default)
    {
        if (conditions != null)
            return await _dbSet!.Where(conditions).Select(selector).ToListAsync(cancellationToken);
        return await _dbSet!.Select(selector).ToListAsync(cancellationToken);
    }


    public async Task<IEnumerable<SelectDto>> GetSelectAsync<TResult>(Expression<Func<TEntity, TResult>> selector)
    {
        return (IEnumerable<SelectDto>)await _dbSet!.Select(selector).ToListAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TEntity> GetByFieldAsync(string filedName, object value,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet!.AsNoTracking()
                   .FirstOrDefaultAsync(e => EF.Property<object>(e, filedName).Equals(value), cancellationToken) ??
               throw new NotFoundException($"{typeof(TEntity).Name} not found");
    }

    public async Task<PaginatedResult<TEntity>> GetPageAsync(
        PaginationRequest paginationRequest,
        CancellationToken cancellationToken = default,
        Expression<Func<TEntity, bool>>? conditions = null)
    {
        if (paginationRequest == null)
        {
            throw new ArgumentNullException(nameof(paginationRequest), "Pagination request cannot be null.");
        }

        int pageIndex = paginationRequest.PageIndex >= 0 ? paginationRequest.PageIndex : 0;
        int pageSize = paginationRequest.PageSize > 0 ? paginationRequest.PageSize : 1;

        IQueryable<TEntity> query = _dbSet!.AsNoTracking();
        if (conditions != null)
        {
            query = query.Where(conditions);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var entities = await query
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pagedResult = new PaginatedResult<TEntity>(pageIndex, pageSize, totalCount, entities);
        return pagedResult;
    }


    public async Task<PaginatedResult<TResult>> GetPageWithIncludesAsync<TResult>(
        PaginationRequest paginationRequest,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? conditions = null,
        List<Expression<Func<TEntity, object>>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet!.AsNoTracking();

        if (conditions != null)
            query = query.Where(conditions);

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var data = await query
            .Skip(paginationRequest.PageSize * paginationRequest.PageIndex)
            .Take(paginationRequest.PageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TResult>(
            data: data,
            pageSize: paginationRequest.PageSize,
            pageIndex: paginationRequest.PageIndex,
            count: totalCount
        );
    }


    public async Task<PaginatedCursorResult<TEntity>> GetPageCursorAsync(
        PaginationCursorRequest paginationRequest,
        CancellationToken cancellationToken = default,
        Expression<Func<TEntity, bool>>? conditions = null,
        Expression<Func<TEntity, long>>? cursorSelector = null)
    {
        if (paginationRequest == null)
        {
            throw new ArgumentNullException(nameof(paginationRequest), "Pagination request cannot be null.");
        }

        int limit = paginationRequest.Limit > 0 ? paginationRequest.Limit : 10;


        long? numericCursor = null;
        if (!string.IsNullOrEmpty(paginationRequest.Cursor) && long.TryParse(paginationRequest.Cursor, out var parsed))
        {
            numericCursor = parsed;
        }

        IQueryable<TEntity> query = _dbSet.AsNoTracking();
        if (conditions != null)
        {
            query = query.Where(conditions);
        }

        if (numericCursor.HasValue && cursorSelector != null)
        {
            query = query.Where(entity => cursorSelector.Compile().Invoke(entity) < numericCursor.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var entities = await query
            .OrderByDescending(cursorSelector)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        long? nextNumericCursor = entities.Count > limit ? cursorSelector.Compile().Invoke(entities[^1]) : null;

        string? nextCursor = nextNumericCursor.HasValue ? nextNumericCursor.Value.ToString() : null;

        return new PaginatedCursorResult<TEntity>(
            paginationRequest.Cursor,
            limit,
            totalCount,
            entities.Take(limit).ToList(),
            nextCursor
        );
    }

    public async Task<PaginatedCursorResult<TResult>> GetPageCursorWithIncludesAsync<TResult>(
        PaginationCursorRequest paginationRequest,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? conditions = null,
        List<Expression<Func<TEntity, object>>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        if (paginationRequest == null)
        {
            throw new ArgumentNullException(nameof(paginationRequest), "Pagination request cannot be null.");
        }

        int limit = paginationRequest.Limit > 0 ? paginationRequest.Limit : 10;


        long? numericCursor = null;
        if (!string.IsNullOrEmpty(paginationRequest.Cursor) && long.TryParse(paginationRequest.Cursor, out var parsed))
        {
            numericCursor = parsed;
        }

        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (conditions != null)
        {
            query = query.Where(conditions);
        }

        if (numericCursor.HasValue)
        {
            query = query.Where(entity => EF.Property<long>(entity, "Id") < numericCursor.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var entities = await query
            .OrderByDescending(entity => EF.Property<long>(entity, "Id"))
            .Take(limit + 1)
            .Select(selector)
            .ToListAsync(cancellationToken);

        long? nextNumericCursor = entities.Count > limit ? EF.Property<long>(entities[^1], "Id") : null;
        string? nextCursor = nextNumericCursor.HasValue ? nextNumericCursor.Value.ToString() : null;

        return new PaginatedCursorResult<TResult>(
            paginationRequest.Cursor,
            limit,
            totalCount,
            entities.Take(limit).ToList(),
            nextCursor
        );
    }

   public async Task<TEntity> GetWithIncludesAsync(
       Expression<Func<TEntity, bool>> expression,
       List<Expression<Func<TEntity, object>>>? includes = null,
       CancellationToken cancellationToken = default)
   {
       IQueryable<TEntity> query = _dbSet!.AsNoTracking();
   
       if (includes != null)
       {
           foreach (var include in includes)
           {
               query = query.Include(include);
           }
       }
   
       var entity = await query.FirstOrDefaultAsync(expression, cancellationToken);
   
       return entity ??
              throw new NotFoundException(
                  $"{typeof(TEntity).Name} not found");
   }


    public async Task<List<TResult>> JoinAsync<TJoin, TKey, TResult>(
        Expression<Func<TEntity, TKey>> outerKeySelector,
        Expression<Func<TJoin, TKey>> innerKeySelector,
        Expression<Func<TEntity, TJoin, TResult>> resultSelector) where TJoin : class
    {
        return await _dbSet.Join(
                _context.Set<TJoin>(),
                outerKeySelector,
                innerKeySelector,
                resultSelector
            )
            .ToListAsync();
    }


    public async Task<List<TResult>> SearchJoinAsync<TJoin, TKey, TResult>(
        Expression<Func<TEntity, TKey>> outerKeySelector,
        Expression<Func<TJoin, TKey>> innerKeySelector,
        Expression<Func<TEntity, TJoin, TResult>> resultSelector,
        Expression<Func<TEntity, bool>>? outerSearchPredicate,
        Expression<Func<TJoin, bool>>? innerSearchPredicate)
        where TJoin : class
    {
        IQueryable<TEntity> outerQuery = _dbSet;
        IQueryable<TJoin> innerQuery = _context.Set<TJoin>();

        if (outerSearchPredicate != null)
        {
            outerQuery = outerQuery.Where(outerSearchPredicate);
        }

        if (innerSearchPredicate != null)
        {
            innerQuery = innerQuery.Where(innerSearchPredicate);
        }

        return await outerQuery
            .Join(innerQuery, outerKeySelector, innerKeySelector, resultSelector)
            .ToListAsync();
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? conditions = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}