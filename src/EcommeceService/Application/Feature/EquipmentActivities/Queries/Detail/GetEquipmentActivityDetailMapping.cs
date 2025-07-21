using Domain.Aggregates.Equipments;

namespace Application.Feature.EquipmentActivities.Queries.Detail
{
    public static class GetEquipmentActivityDetailMapping
    {
        public static GetEquipmentActivityDetailResponse ToEquipmentActivityDetailResponse(
            this EquipmentActivity activity
        )
        {
            var response = new GetEquipmentActivityDetailResponse();
            response.MappingFrom(activity);
            return response;
        }
    }
}
