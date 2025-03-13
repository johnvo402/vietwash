using Application.Features.Common.Projections.Users;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserCommand : IRequest<UpdateUserResponse>
{
    [FromRoute(Name = RouterBase.Id)]
    public string UserId { get; set; } = string.Empty;

    [FromForm]
    public UpdateUser? User { get; set; }
}

public class UpdateUser : UserModel
{
    public Ulid RoleId { get; set; }
}
