using Application.Feature.Common.Projections.Categories;
using Domain.Aggregates.Services;

namespace Application.Feature.Common.Mapping.Categories
{
    public static class CategoryMapping
    {
        public static CategoryProjection ToCategoryProjectionResponse(this Category category)
        {
            return new()
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                Path = category.Path,
                ParentId = category.ParentId,
                Status = category.Status,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
            };
        }
    }
}
