using System.Linq.Expressions;
using System.Text.Json;
using Micro.Shared.Model;
using Micro.Shared.Queries;

namespace Micro.Shared.Queries;

public class OrderBySpecification<T> : IQuerySpecification<T>
{
    private readonly Dictionary<string, string>? _orderBy;

    public OrderBySpecification(Dictionary<string, string>? orderBy)
    {
        _orderBy = orderBy;
    }

    public IQueryable<T> Apply(IQueryable<T> query)
    {
        if (_orderBy == null)
            return query;

        try
        {
            if (_orderBy == null)
                return query;

            var isFirst = true;
            foreach (var order in _orderBy)
            {
                var property = typeof(T).GetProperty(order.Key);
                if (property == null) continue;

                var param = Expression.Parameter(typeof(T), "x");
                var propAccess = Expression.Property(param, property);
                var orderByExp = Expression.Lambda(propAccess, param);

                string methodName;
                if (isFirst)
                {
                    methodName = order.Value.ToUpper() == "DESC" ? "OrderByDescending" : "OrderBy";
                    isFirst = false;
                }
                else
                {
                    methodName = order.Value.ToUpper() == "DESC" ? "ThenByDescending" : "ThenBy";
                }

                var resultExp = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new Type[] { typeof(T), property.PropertyType },
                    query.Expression,
                    Expression.Quote(orderByExp)
                );

                query = query.Provider.CreateQuery<T>(resultExp);
            }

            return query;
        }
        catch
        {
            return query;
        }
    }
}
