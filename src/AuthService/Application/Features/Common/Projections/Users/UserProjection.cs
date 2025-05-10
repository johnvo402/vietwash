using Application.Common.Security;
using Domain.Aggregates.Users.Enums;
using JohnChum.SharedKernel.Application.Common;
using System.Text.Json.Serialization;

namespace Application.Features.Common.Projections.Users;

public class UserProjection : BaseResponse
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? DayOfBirth { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Gender? Gender { get; set; }

    [File]
    public string? Avatar { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserStatus Status { get; set; }
    public string? Role { get; set; }
}
