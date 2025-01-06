using System.Linq.Expressions;

namespace BuildingBlocks.Abstractions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, params Expression<Func<T, bool>>[] predicates)
    {
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }
        return query;
    }
}
