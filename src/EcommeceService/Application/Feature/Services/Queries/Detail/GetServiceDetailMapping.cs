using Domain.Aggregates.Services;

namespace Application.Feature.Services.Queries.Detail
{
    public static class GetServiceDetailMapping
    {
        public static GetServiceDetailResponse GetDetailSelector(this Service service)
        {
            GetServiceDetailResponse response = new GetServiceDetailResponse();
            response.MappingFrom(service);
            response.CategoryId = service.CategoryId;

            return response;
        }
    }
}
