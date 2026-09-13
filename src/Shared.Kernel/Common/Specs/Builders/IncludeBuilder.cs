using System.Linq.Expressions;
using Shared.Kernel.Common.Specs.Interfaces;
using Shared.Kernel.Common.Specs.Models;
using Specification.Models;

namespace Specification.Builders;

public static class IncludeBuilder
{
    public static IIncludableSpecificationBuilder<T, TProperty> Include<T, TProperty>(
        this ISpecificationBuilder<T> builder,
        Expression<Func<T, TProperty>> includeExpression
    )
        where T : class
    {
        IncludeInfo includeInfo = new()
        {
            EntityType = typeof(T),
            InCludeType = InCludeType.Include,
            LamdaExpression = includeExpression,
            PropertyType = typeof(TProperty),
        };

        builder.Spec!.Includes.Add(includeInfo);

        return new IncludableSpecificationBuilder<T, TProperty>(builder.Spec);
    }

    public static IIncludableSpecificationBuilder<T, TProperty> ThenInclude<
        T,
        TPreviousProperty,
        TProperty
    >(
        this IIncludableSpecificationBuilder<T, TPreviousProperty> builder,
        Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression
    )
        where T : class
    {
        return ThenIncludeBase(thenIncludeExpression, builder);
    }

    public static IIncludableSpecificationBuilder<T, TProperty> ThenInclude<
        T,
        TPreviousProperty,
        TProperty
    >(
        this IIncludableSpecificationBuilder<T, ICollection<TPreviousProperty>> builder,
        Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression
    )
        where T : class
    {
        return ThenIncludeBase(thenIncludeExpression, Collectionbuilder: builder);
    }

    public static IIncludableSpecificationBuilder<T, TProperty> ThenInclude<
        T,
        TPreviousProperty,
        TProperty
    >(
        this IIncludableSpecificationBuilder<T, IReadOnlyCollection<TPreviousProperty>> builder,
        Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression
    )
        where T : class
    {
        return ThenIncludeBase(thenIncludeExpression, readOnlyCollectionBuilder: builder);
    }

    private static IIncludableSpecificationBuilder<T, TProperty> ThenIncludeBase<
        T,
        TPreviousProperty,
        TProperty
    >(
        Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression,
        IIncludableSpecificationBuilder<T, TPreviousProperty> builder = null!,
        IIncludableSpecificationBuilder<T, ICollection<TPreviousProperty>> Collectionbuilder = null!,
        IIncludableSpecificationBuilder<T, IReadOnlyCollection<TPreviousProperty>> readOnlyCollectionBuilder = null!
    )
        where T : class
    {
        IncludeInfo includeInfo = new()
        {
            EntityType = typeof(T),
            InCludeType = InCludeType.ThenInclude,
            LamdaExpression = thenIncludeExpression,
            PreviousPropertyType = typeof(TPreviousProperty),
            PropertyType = typeof(TProperty),
        };
        Specification<T>? Spec =
            builder?.Spec ?? Collectionbuilder?.Spec ?? readOnlyCollectionBuilder.Spec;
        Spec!.Includes.Add(includeInfo);

        IIncludableSpecificationBuilder<T, TProperty> includeBuilder =
            new IncludableSpecificationBuilder<T, TProperty>(Spec);
        return includeBuilder;
    }
}
