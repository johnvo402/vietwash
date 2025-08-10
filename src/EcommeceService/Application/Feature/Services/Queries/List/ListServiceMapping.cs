using System.Linq.Expressions;
using Application.Feature.Common.Mapping.Units;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Queries.List;

public class ListServiceMapping
{
    public static Expression<Func<Service, ListServiceResponse>> Selector()
    {
        return service => new ListServiceResponse
        {
            Id = service.Id,
            PublicId = service.PublicId,
            CreatedAt = service.CreatedAt,
            CreatedBy = service.CreatedBy,
            UpdatedAt = service.UpdatedAt,
            UpdatedBy = service.UpdatedBy,

            // Từ ServiceProjection
            Name = service.Name,
            Image = service.Image,
            Status = service.Status,
            CategoryId = service.CategoryId,
            // Navigation properties
            Category =
                service.Category == null
                    ? null
                    : new CategoryService { Name = service.Category.Name },
            UnitRelations = service
                .UnitRelations.Select(x => x.ToUnitRelationProjectionResponse())
                .ToList(),
        };
    }
}
