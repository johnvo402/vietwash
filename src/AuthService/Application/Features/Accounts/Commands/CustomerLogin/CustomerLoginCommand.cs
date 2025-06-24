using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Accounts.Commands.CustomerLogin;

public class CustomerLoginCommand : IRequest<Result>
{
    public string PhoneNumber { get; set; } = null!;
}
