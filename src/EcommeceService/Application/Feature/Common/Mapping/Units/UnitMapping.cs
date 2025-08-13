using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Services;

namespace Application.Feature.Common.Mapping.Units
{
    public static class UnitMapping
    {
        public static Unit FromUpdateUnit(this Unit unit, UnitModel model)
        {
            unit.Update(name: model.Name, status: model.Status);
            return unit;
        }

        public static UnitRelation FromUpdateRelation(
            this UnitRelation entity,
            UnitRelationModel model
        )
        {
            entity.Update(
                name: model.Name,
                baseUnit: model.BaseUnit,
                price: model.Price,
                multiple: model.Multiple,
                processingTime: model.ProcessingTime,
                status: model.Status,
                unitId: model.UnitId
            );
            return entity;
        }

        public static List<ServiceResource>? ToListServiceResource(
            this List<ServiceResourceModel>? serviceResources
        ) => serviceResources?.Select(ToServiceResourceEntity).ToList();

        public static ServiceResource ToServiceResourceEntity(this ServiceResourceModel model)
        {
            return new ServiceResource
            {
                ProductId = model.ProductId,
                UnitProductId = model.UnitProductId,
                Quantity = model.Quantity,
            };
        }

        public static List<UnitRelation>? ToListUnitRelation(
            this List<UnitRelationModel>? unitRelations
        ) => unitRelations?.Select(ToUnitRelationEntity).ToList();

        public static UnitRelation ToUnitRelationEntity(this UnitRelationModel model)
        {
            return new UnitRelation
            {
                Name = model.Name,
                BaseUnit = model.BaseUnit,
                Price = model.Price,
                Multiple = model.Multiple,
                ProcessingTime = model.ProcessingTime,
                Status = model.Status,
                UnitId = model.UnitId,
                AsUnitProduct = model.ServiceResources.ToListServiceResource() ?? [],
            };
        }

        public static UnitRelationProjection ToUnitRelationProjectionResponse(this UnitRelation ur)
        {
            return new()
            {
                Id = ur.Id,
                Name = ur.Name,
                BaseUnit = ur.BaseUnit,
                Price = ur.Price,
                Multiple = ur.Multiple,
                ProcessingTime = ur.ProcessingTime,
                Status = ur.Status,
                ServiceResources = ur
                    .AsUnitRelation.Select(x => x.ToServiceResourceProjectionResponse())
                    .ToList(),
            };
        }

        public static ServiceResourceProjection ToServiceResourceProjectionResponse(
            this ServiceResource serviceResource
        )
        {
            return new()
            {
                UnitProductId = serviceResource.UnitProductId,
                ProductId = serviceResource.ProductId,
                ProductName = serviceResource.BranchProduct.Name,
                UnitName = serviceResource.UnitProduct.Name,
                Quantity = serviceResource.Quantity,
            };
        }
    }
}
