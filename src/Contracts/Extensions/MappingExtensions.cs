using System.Linq.Expressions;

namespace Contracts.Extensions
{
    public static class MappingExtensions
    {
        public static ICollection<TDestination> ToListMapping<TSource, TDestination>(
            this ICollection<TSource>? source,
            Func<TSource, TDestination> selector
        )
        {
            return source?.Select(selector).ToList() ?? [];
        }

        public static IQueryable<TDestination> ProjectTo<TSource, TDestination>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TDestination>> selector
        )
        {
            return source.Select(selector);
        }
    }
}
