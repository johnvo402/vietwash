using Application.Features.Common.Projections.Users;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users.Enums;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommand
    : QueueBasePayload<CreateAccountEvent>,
        IRequest<QueueResponse<CreateUserCommand>>;

public class CreateAccountEvent
{
    public Ulid Id { get; set; }
    public string? Username { get; set; }

    public string? Password { get; set; }

    public Gender? Gender { get; set; }

    public UserStatus Status { get; set; }

    public string RoleId { get; set; }
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? DayOfBirth { get; set; }

    public string? ProvinceId { get; set; }

    public string? DistrictId { get; set; }

    public string? CommuneId { get; set; }

    public string? Street { get; set; }

    public IFormFile? Avatar { get; set; }
}
