using Contracts.Extensions;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Users;

namespace Application.Feature.Equipments.Command.CreateActivities
{
    public static class CreateEquipmentActivityMapping
    {
        public static EquipmentActivity ToEquipmentActivity(
            this CreateEquipmentActivityCommand cmd,
            User staff
        )
        {
            decimal amount = cmd.EquipmentActivity.Details.Sum(d => d.UnitPrice * d.Quantity);

            var response = new EquipmentActivity(
                equipmentId: cmd.Id,
                staffId: staff.Id,
                type: cmd.EquipmentActivity.Type,
                laborCost: cmd.EquipmentActivity.LaborCost,
                totalCost: amount + cmd.EquipmentActivity.LaborCost,
                description: cmd.EquipmentActivity.Description
            );
            response.ActivityDetails = cmd.EquipmentActivity.Details.ToListMapping(
                x => new EquipmentActivityDetail
                {
                    PartName = x.PartName,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    Amount = x.Quantity * x.UnitPrice,
                }
            );

            return response;
        }
    }
}
