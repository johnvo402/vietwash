using Application.Common.Security;

namespace Application.Features.Common.Projections.Accounts;

public class AccountModel
{
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? BirthDay { get; set; }

    [File]
    public string? AvtUrl { get; set; }
}
