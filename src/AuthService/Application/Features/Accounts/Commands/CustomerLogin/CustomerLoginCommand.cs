using Mediator;

namespace Application.Features.Accounts.Commands.CustomerLogin;

public class CustomerLoginCommand : IRequest<CustomerLoginResponse>
{
    public string PhoneNumber { get; set; } = null!;
}
