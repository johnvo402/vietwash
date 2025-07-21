using Application.Feature.Common.Projections.Equipments;
using Domain.Aggregates.Equipments;

namespace Application.Feature.Equipments.Command.Update
{
    public static class UpdateEquipmentMapping
    {
        public static void FromUpdateModel(this Equipment entity, EquipmentUpdateModel model)
        {
            entity.Update(name: model.Name, description: model.Description, status: model.Status);
            entity.Image = model.Image;
        }
    }
}
