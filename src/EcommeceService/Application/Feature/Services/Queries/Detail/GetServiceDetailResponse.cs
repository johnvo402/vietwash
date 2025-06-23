using Application.Feature.Common.Projections.Services;
using Domain.Aggregates.Users.Enums;

namespace Application.Feature.Services.Queries.Detail
{
    public class GetServiceDetailResponse : ServiceDetailProjection
    {
        //public UserDTO? CreatedByUser { get; set; }
        //public UserDTO? UpdatedByUser { get; set; }\
        public string? CategoryId { get; set; }
    }

    public class UserDTO
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public CustomerGroup? CustomerGroup { get; set; }
    }
}
