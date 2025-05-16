using Microsoft.AspNetCore.Http;

namespace Application.Features.Common.Projections.Accounts;

public class AccountModel
{
    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? DayOfBirth { get; set; }

    public IFormFile? Avatar { get; set; }
}
