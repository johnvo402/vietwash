using System.Linq.Expressions;
using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Queries.GetByTariff
{
    public static class GetByTariffMapping
    {
        public static Expression<Func<Category, GetByTariffResponse>> Selector()
        {
            return category => new GetByTariffResponse
            {
                Id = category.Id,
                Name = category.Name,
                Services = category
                    .Services.Select(service => new ServiceProjection
                    {
                        Id = service.Id,
                        Name = service.Name,
                        Image = service.Image,
                        Status = service.Status,
                        CategoryId = service.CategoryId,
                        UnitRelations = service
                            .UnitRelations.Select(ur => new UnitRelationProjection
                            {
                                Id = ur.Id,
                                Name = ur.Name,
                                BaseUnit = ur.BaseUnit,
                                Price = ur.Price,
                                Multiple = ur.Multiple,
                                ProcessingTime = ur.ProcessingTime,
                                Status = ur.Status,
                            })
                            .ToList(),
                    })
                    .ToList(),
            };
        }
    }
}
