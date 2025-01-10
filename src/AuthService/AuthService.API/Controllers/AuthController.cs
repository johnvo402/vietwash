using MediatR;
using Microsoft.AspNetCore.Mvc;
using AuthService.Application.Auth.Commands.Login;
using Microsoft.AspNetCore.Authorization;
using AuthService.Application.Auth.Queries.GetMe;
using AuthService.Application.Auth.Commands.RefreshToken;
using AuthService.Application.Interfaces;
using AuthService.Domain.Roles;
using AuthService.Domain.ValueObjects;


namespace AuthService.API.Controllers;

[ApiVersion("1.0")]
[Route("auth/api/v1/auth")]

public class AuthController(ISender _mediator, IRoleRepo _roleManager) : ApiController
{

    [HttpPost("init")]
    [AllowAnonymous]
    public async Task<IActionResult> Init(CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Role> roles = new List<Role>
        {
            new Role { RoleName = RoleNameValues.Admin, OrgId = "DOAN" },
            new Role { RoleName = RoleNameValues.Manager, OrgId = "DOAN" },
            new Role { RoleName = RoleNameValues.Staff, OrgId = "DOAN" },
            new Role { RoleName = RoleNameValues.Customer, OrgId = "DOAN" }
        };

            await _roleManager.BulkAddAsync(roles, cancellationToken);


            return Ok("Init success");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }




    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match(
           user => Ok(user),
           Problem);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(GetMeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me()
    {
        var result = await _mediator.Send(new GetMeQuery());
        return result.Match(
           user => Ok(user),
           Problem);
    }


    
    [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefeshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match(
         token => Ok(token),
         Problem);
    }
}
