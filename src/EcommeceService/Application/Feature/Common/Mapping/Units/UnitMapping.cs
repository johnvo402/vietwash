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
                status: model.Status
            );
            return entity;
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
            };
        }
    }
}
