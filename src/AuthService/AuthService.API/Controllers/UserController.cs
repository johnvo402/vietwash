using AuthService.Application.Auth.Commands.CreateUserCommands;
using AuthService.Application.Users.Commands.UpdateUserCommands;
using AuthService.Application.Users.Queries.GetUserQueries;
using AuthService.Domain.Users.Entity;
using MediatR;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;
[ApiVersion("1.0")]
[Route("auth/api/v1/user")]
public class UserController(ISender _mediator) : ApiController
{
  [HttpGet]
  [AllowAnonymous]
  public async Task<IActionResult> Get([FromQuery] QueryParameters? request)
  {
    var query = new GetUserQuery(request);
    var result = await _mediator.Send(query);
    return result.Match(
      user => Ok(user),
      Problem);
  }
  [HttpGet("{id}")]
  [AllowAnonymous]
  public async Task<IActionResult> GetById(Guid id)
  {
    var param = new QueryParameters();
    param.Where = $"id = '{id}'";
    var query = new GetUserQuery(param);
    var result = await _mediator.Send(query);
    return result.Match(
      user => Ok(user?.Data?.FirstOrDefault()),
      Problem);
  }

  [HttpPost("create")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [AllowAnonymous]
  public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
  {
    var result = await _mediator.Send(command);
    return result.Match(
      user => Ok(user),
      Problem);
  }
  [HttpPut("update")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [AllowAnonymous]
  public async Task<IActionResult> Update([FromBody] UpdateUserCommand command)
  {
    var result = await _mediator.Send(command);
    return result.Match(
      user => Ok(user),
      Problem);
  }
  [HttpPatch("update-many")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [AllowAnonymous]
  public async Task<IActionResult> UpdateMany([FromBody] UpdateUserManyCommand command)
  {
    var result = await _mediator.Send(command);
    return result.Match(
      user => Ok(user),
      Problem);
  }
}