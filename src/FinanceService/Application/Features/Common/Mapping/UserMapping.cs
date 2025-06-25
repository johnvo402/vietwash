using Application.Features.Common.Projections.Users;
using Domain.Aggregates.Users;

namespace Application.Features.Common.Mapping
{
    public static class UserMapping
    {
        public static UserDTO ToUserDTOResponse(this User order)
        {
            var response = new UserDTO();
            response.MappingFrom(order);
            return response;
        }
    }
}
