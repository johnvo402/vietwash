using Application.Common.Security;
using Application.Feature.Common.Projections.Services;
using Domain.Aggregates.Users.Enums;

namespace Application.Feature.Services.Queries.Detail
{
    public class GetServiceDetailResponse : ServiceDetailProjection
    {
        public UserDTO? CreatedUser { get; set; }
        public UserDTO? UpdatedUser { get; set; }
    }

    public class UserDTO
    {
        public long? Id { get; set; }
        public Ulid? PublicId { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public CustomerGroup? CustomerGroup { get; set; }
		[File]
		public string? Avatar { get; set; } 
	}
}
