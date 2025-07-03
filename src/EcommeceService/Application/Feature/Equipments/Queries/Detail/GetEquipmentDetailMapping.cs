using Domain.Aggregates.Equipments;

namespace Application.Feature.Equipments.Queries.Detail;

public static class GetEquipmentDetailMapping
{
	public static GetEquipmentDetailResponse ToGetEquipmentDetailResponse(this Equipment equipment)
	{
		var response = new GetEquipmentDetailResponse();
		response.MappingFrom(equipment);

		return response;
	}
}
