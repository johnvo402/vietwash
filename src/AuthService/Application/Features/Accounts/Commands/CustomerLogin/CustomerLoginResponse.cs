using Application.Features.Common.Projections;

namespace Application.Features.Accounts.Commands.CustomerLogin;

public class CustomerLoginResponse : MessageOutput
{
    public string? Key { get; set; }
    public long? AccountId { get; set; }
};
