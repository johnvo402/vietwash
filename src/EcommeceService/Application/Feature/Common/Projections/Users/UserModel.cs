using Domain.Aggregates.Users.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Common.Projections.Users;

public class UserModel
{
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }

    public DateTime? Birthday { get; set; }
    public string? AvtUrl { get; set; }
}
