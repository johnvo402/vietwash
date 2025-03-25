using AutoMapper;
using Domain.Aggregates.Users;
using static Application.Common.DomainEventHandlers.UserCreateEventHandler;

namespace Application.Common.DomainEventHandlers
{
    internal class UserCreateEventMapping : Profile
    {
        public UserCreateEventMapping()
        {
            CreateMap<User, CreateUserEvent>();
        }
    }
}
