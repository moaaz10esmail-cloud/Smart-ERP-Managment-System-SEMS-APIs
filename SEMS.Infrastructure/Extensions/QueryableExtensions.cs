using System.Linq.Expressions;
using SEMS.Core.Common;

namespace SEMS.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }
        var param = Expression.Parameter(typeof(T), "x");
        var nameProp = typeof(T).GetProperty("Name");
        if (nameProp != null && nameProp.PropertyType == typeof(string))
        {
            var prop = Expression.Property(param, nameProp);
            var toLower = Expression.Call(prop, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
            var termConst = Expression.Constant(searchTerm.ToLower());
            var contains = Expression.Call(toLower, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, termConst);
            var lambda = Expression.Lambda<Func<T, bool>>(contains, param);
            return query.Where(lambda);
        }
        return query;
    }
    
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, PagedQuery pagedQuery)
    {
        if (pagedQuery.Page <= 0) pagedQuery.Page = 1;
        if (pagedQuery.PageSize <= 0) pagedQuery.PageSize = 10;

        return query.Skip((pagedQuery.Page - 1) * pagedQuery.PageSize).Take(pagedQuery.PageSize);
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query;
        }

        var param = Expression.Parameter(typeof(T), "x");
        
        // Handle nested properties (e.g. "Department.Name")
        Expression conversion = param;
        foreach (var member in sortBy.Split('.'))
        {
            conversion = Expression.PropertyOrField(conversion, member);
        }

        var keySelector = Expression.Lambda(conversion, param);

        var methodName = sortDirection?.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";
        
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), conversion.Type },
            query.Expression,
            Expression.Quote(keySelector));

        return query.Provider.CreateQuery<T>(resultExpression);
    }
}
