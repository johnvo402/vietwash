using Application.Features.Common.Projections.Users;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserCommand : IRequest<Result<UpdateUserResponse>>
{
    [FromRoute(Name = RouterBase.Id)]
    public long UserId { get; set; }

    [FromForm]
    public UpdateAccount? User { get; set; }
}

public class UpdateAccount : UserModel
{
    public string? Email { get; set; }

    public Gender? Gender { get; set; }

    public ActivationStatus Status { get; set; }

    public string Role { get; set; } = default!;
    public CustomerGroup? CustomerGroup { get; set; }
}
