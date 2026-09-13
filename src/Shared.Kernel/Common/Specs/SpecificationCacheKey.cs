using Shared.Kernel.Common.Specs.Interfaces;

namespace Specification;

internal static class SpecificationCacheKey
{
    public static string Create<T>(ISpecification<T> specification)
        where T : class
    {
        IEnumerable<string> criteria = specification.Criteria.Select(x => x.Criteria.ToString());
        IEnumerable<string> includes = specification.Includes.Select(x =>
            $"{x.InCludeType}:{x.LamdaExpression}"
        );
        IEnumerable<string> sorts = specification.Sorts.Select(x =>
            $"{x.OrderType}:{x.IsThenBy}:{x.KeySelector}"
        );

        return string.Join(
            "~",
            criteria
                .Concat(includes)
                .Concat(sorts)
                .Append($"skip:{specification.Skip}")
                .Append($"take:{specification.Take}")
                .Append($"split:{specification.IsSplitQuery}")
                .Append($"tracking:{specification.IsNoTracking}")
        );
    }
}
