using System.Linq.Expressions;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination;

namespace BuildingBlocks.RepositoryBase.EntityFramework;

public interface IRepositoryBase<TEntity> where TEntity : class
{
    IQueryable<TEntity> Query();
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default!);

    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default!);

    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> func, CancellationToken cancellationToken = default!);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default!);

    Task UpdateAsync(Expression<Func<TEntity, bool>> func, Object payload,
        CancellationToken cancellationToken = default!);

    Task DeleteAsync(Expression<Func<TEntity, bool>> func, CancellationToken cancellationToken = default!);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default!);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default!);

    Task<IEnumerable<TResult>> GetSelectAsync<TResult>(Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? conditions, CancellationToken cancellationToken = default!);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default!);

    Task<TEntity> GetByFieldAsync(string filedName, object value, CancellationToken cancellationToken = default!);

    Task<PaginatedResult<TEntity>> GetPageAsync(PaginationRequest paginationRequest,
        CancellationToken cancellationToken = default!, Expression<Func<TEntity, bool>>? conditions = null);

    Task<PaginatedResult<TResult>> GetPageWithIncludesAsync<TResult>(
        PaginationRequest paginationRequest,
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? conditions = null,
        List<Expression<Func<TEntity, object>>>? includes = null,
        CancellationToken cancellationToken = default);

    Task<TEntity> GetByFieldWithIncludesAsync(
        string fieldName,
        object value,
        List<Expression<Func<TEntity, object>>>? includes = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<SelectDto>> GetSelectAsync<TResult>(Expression<Func<TEntity, TResult>> selector);

    Task<List<TResult>> JoinAsync<TJoin, TKey, TResult>(
        Expression<Func<TEntity, TKey>> outerKeySelector,
        Expression<Func<TJoin, TKey>> innerKeySelector,
        Expression<Func<TEntity, TJoin, TResult>> resultSelector)  where TJoin : class;

    Task<List<TResult>> SearchJoinAsync<TJoin, TKey, TResult>(
        Expression<Func<TEntity, TKey>> outerKeySelector,
        Expression<Func<TJoin, TKey>> innerKeySelector,
        Expression<Func<TEntity, TJoin, TResult>> resultSelector,
        Expression<Func<TEntity, bool>> outerSearchPredicate, 
        Expression<Func<TJoin, bool>>? innerSearchPredicate) 
        where TJoin : class;
}