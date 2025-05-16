using AutoMapper;
using Domain.Aggregates.Accounts;
using static Application.Common.DomainEventHandlers.AccountCreateEventHandler;

namespace Application.Common.DomainEventHandlers
{
    internal class AccountCreateEventMapping : Profile
    {
        public AccountCreateEventMapping()
        {
            CreateMap<Account, CreateAccountEvent>();
        }
    }
}
