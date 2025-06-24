using Application.Features.Common.Projections.Users;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserCommand : IRequest<Result<UpdateUserResponse>>
{
    [FromRoute(Name = RouterBase.Id)]
    public long UserId { get; set; }

    [FromForm]
    public UserModel? User { get; set; }
}
