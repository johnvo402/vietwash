using Application.Features.Common.Projections.Users;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users.Enums;
using Mediator;

namespace Application.Features.Users.Commands.Create;

public class CreateAccountCommand
    : QueueBasePayload<CreateAccountEvent>,
        IRequest<QueueResponse<CreateAccountCommand>>;
public class CreateAccountEvent
{
    public long Id { get; set; }

    public string? Password { get; set; }

    public Gender? Gender { get; set; }

    public UserStatus Status { get; set; }

    public string Role { get; set; }
    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateOnly? BirthDay { get; set; }

    public string? Avatar { get; set; }
}