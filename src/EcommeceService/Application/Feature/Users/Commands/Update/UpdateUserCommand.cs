using Application.Features.Common.Projections.Users;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserCommand : IRequest<UpdateUserResponse>
{
    [FromRoute(Name = RouterBase.Id)]
    public long UserId { get; set; }

    [FromForm]
    public UpdateUser? User { get; set; }
}

public class UpdateUser : UserModel
{
}
