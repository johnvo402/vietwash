using System.Linq.Expressions;
using Domain.Aggregates.Services;

namespace Application.Feature.Categories.Queries.List;

public class ListCategoryMapping
{
    public static Expression<Func<Category, ListCategoryResponse>> Selector()
    {
        return c => new ListCategoryResponse
        {
            Id = c.Id,
            CreatedAt = c.CreatedAt,
            CreatedBy = c.CreatedBy,
            UpdatedAt = c.UpdatedAt,
            UpdatedBy = c.UpdatedBy,

            Name = c.Name,
            Path = c.Path,
            ParentId = c.ParentId,
            Status = c.Status,
        };
    }
}
