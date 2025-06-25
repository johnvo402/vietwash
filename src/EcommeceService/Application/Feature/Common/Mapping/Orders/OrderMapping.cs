using Application.Feature.Services.Queries.Detail;
using Domain.Aggregates.Users;

namespace Application.Feature.Common.Mapping.Orders
{
    public static class OrderMapping
    {
        public static UserDTO? MappingFrom(this User? user)
        {
            if (user == null)
                return null;
            return new UserDTO
            {
                Id = user.Id,
                PublicId = user.PublicId,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };
        }
    }
}
