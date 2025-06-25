using Application.Features.Common.Projections.Users;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users.Enums;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommand
    : PubSubBasePayload<CreateAccountEvent>,
        IRequest<PubSubResponse<CreateUserCommand>>;

public class CreateAccountEvent
{
    public long Id { get; set; }
    public Ulid PublicId { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Code { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly BirthDay { get; set; }
    public Gender? Gender { get; set; }
    public string? AvtUrl { get; set; }
    public string? Role { get; set; }
    public bool Disabled { get; set; }
    public CustomerGroup? CustomerGroup { get; set; }

    public UserStatus Status { get; set; }
}
