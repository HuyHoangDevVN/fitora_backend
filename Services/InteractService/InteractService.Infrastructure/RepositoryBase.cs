using System.Linq.Expressions;
using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
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
        //return await GetAllCompiledQuery(_context);
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
        _dbSet.Remove(entity);
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
        Expression<Func<TEntity, long>>? cursorSelector = null) // Thêm cursorSelector để xác định tiêu chí phân trang
    {
        if (paginationRequest == null)
        {
            throw new ArgumentNullException(nameof(paginationRequest), "Pagination request cannot be null.");
        }

        int limit = paginationRequest.Limit > 0 ? paginationRequest.Limit : 10;
        long? cursor = paginationRequest.Cursor;

        IQueryable<TEntity> query = _dbSet!.AsNoTracking();
        if (conditions != null)
        {
            query = query.Where(conditions);
        }

        if (cursor.HasValue && cursorSelector != null)
        {
            query = query.Where(entity => cursorSelector.Compile().Invoke(entity) < cursor.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var entities = await query
            .OrderByDescending(cursorSelector) // Phân trang kiểu cursor
            .Take(limit + 1) // Lấy thêm 1 phần tử để kiểm tra nextCursor
            .ToListAsync(cancellationToken);

        long? nextCursor = entities.Count > limit ? cursorSelector.Compile().Invoke(entities[^1]) : null;

        return new PaginatedCursorResult<TEntity>(cursor, limit, totalCount, entities.Take(limit).ToList(), nextCursor);
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
        long? cursor = paginationRequest.Cursor;

        IQueryable<TEntity> query = _dbSet!.AsNoTracking();

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

        if (cursor.HasValue)
        {
            query = query.Where(entity => EF.Property<long>(entity, "Id") < cursor.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var entities = await query
            .OrderByDescending(entity => EF.Property<long>(entity, "Id")) // Cursor dựa vào ID
            .Take(limit + 1)
            .Select(selector)
            .ToListAsync(cancellationToken);

        long? nextCursor = entities.Count > limit ? EF.Property<long>(entities[^1], "Id") : null;
        return new PaginatedCursorResult<TResult>(cursor, limit, totalCount, entities.Take(limit).ToList(), nextCursor);
    }


    public async Task<TEntity> GetByFieldWithIncludesAsync(
        string fieldName,
        object value,
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

        var entity = await query.FirstOrDefaultAsync(
            e => EF.Property<object>(e, fieldName).Equals(value),
            cancellationToken
        );

        return entity ?? throw new NotFoundException($"{typeof(TEntity).Name} with {fieldName} '{value}' not found");
    }


    // Generic join method
    public async Task<List<TResult>> JoinAsync<TJoin, TKey, TResult>(
        Expression<Func<TEntity, TKey>> outerKeySelector,
        Expression<Func<TJoin, TKey>> innerKeySelector,
        Expression<Func<TEntity, TJoin, TResult>> resultSelector) where TJoin : class
    {
        return await _dbSet.Join(
                _context.Set<TJoin>(), // Inner set
                outerKeySelector, // Outer key selector
                innerKeySelector, // Inner key selector
                resultSelector // Result selector
            )
            .ToListAsync();
    }

    // Generic join with search
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