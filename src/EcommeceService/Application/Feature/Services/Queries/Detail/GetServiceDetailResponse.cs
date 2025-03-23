using Application.Feature.Common.Projections.Services;

namespace Application.Feature.Services.Queries.Detail
{
    public class GetServiceDetailResponse : ServiceDetailProjection
    {
        public UserDTO? CreatedByUser { get; set; }
        public UserDTO? UpdatedByUser { get; set; }
    }

    public class UserDTO
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }
}
