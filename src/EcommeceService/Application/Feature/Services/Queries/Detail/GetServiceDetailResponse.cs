using Application.Feature.Common.Projections.Services;
using Application.Features.Common.Projections.Users;

namespace Application.Feature.Services.Queries.Detail
{
    public class GetServiceDetailResponse : ServiceDetailProjection
    {
        public UserDTO? CreatedUser { get; set; }
        public UserDTO? UpdatedUser { get; set; }
    }
}
