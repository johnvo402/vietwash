using AuthService.Application.Permissions.Commands.PermissionCreateCommands;
using AuthService.Application.Permissions.Queries;
using AuthService.Application.Roles.Commands.RoleCreateCommads;
using AuthService.Application.Roles.Queries;
using MediatR;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;
[ApiVersion("1.0")]
[Route("auth/api/v1/permission")]
public class PermissionController(ISender _mediator) : ApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get([FromQuery] QueryParameters? request)
    {
        var query = new PermissionQuery(request);
        var result = await _mediator.Send(query);
        return result.Match(
          user => Ok(user),
          Problem);
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] PermissionCreateCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match(
          user => Ok(user),
          Problem);
    }
    // [HttpPut("update")]
    // [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    // [AllowAnonymous]
    // public async Task<IActionResult> Update([FromBody] UpdateUserCommand command)
    // {
    //     var result = await _mediator.Send(command);
    //     return result.Match(
    //       user => Ok(user),
    //       Problem);
    // }
    // [HttpPatch("update-many")]
    // [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    // [AllowAnonymous]
    // public async Task<IActionResult> UpdateMany([FromBody] UpdateUserManyCommand command)
    // {
    //     var result = await _mediator.Send(command);
    //     return result.Match(
    //       user => Ok(user),
    //       Problem);
    // }
}