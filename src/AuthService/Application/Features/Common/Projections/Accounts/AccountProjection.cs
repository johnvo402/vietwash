using System.Text.Json.Serialization;
using Application.Common.Security;
using Domain.Aggregates.Accounts.Enums;
using JohnChum.SharedKernel.Application.Common;

namespace Application.Features.Common.Projections.Accounts;

public class AccountProjection : BaseResponse
{
    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateOnly? BirthDay { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Gender? Gender { get; set; }

    [File]
    public string? AvtUrl { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AccountStatus Status { get; set; }
    public string? Role { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AccountLanguages? Language { get; set; }
}
