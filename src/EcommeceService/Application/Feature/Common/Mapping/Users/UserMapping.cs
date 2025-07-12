using Application.Features.Common.Projections.Users;
using Domain.Aggregates.Users;

namespace Application.Features.Common.Mapping.Users;

public static class UserMapping
{
    public static UserDTO UserDTOResponse(this User order)
    {
        var response = new UserDTO();
        response.MappingFrom(order);
        return response;
    }
}
