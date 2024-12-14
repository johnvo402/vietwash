using MediatR;
using Microsoft.AspNetCore.Mvc;
using AuthService.Application.Commands;
using AuthService.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Micro.Shared.Data;

namespace AuthService.API.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly RoleManager<Role> _roleManager;
    public AuthController(IMediator mediator, RoleManager<Role> roleManager)
    {
        _mediator = mediator;
        _roleManager = roleManager;
    }

    [HttpGet("init")]
    public async Task<IActionResult> Init()
    {
        var rolenames = typeof(RoleName).GetFields().ToList();
        foreach (var r in rolenames)
        {
            var rolename = (string)r.GetRawConstantValue();
            var rfound = await _roleManager.FindByNameAsync(rolename);
            if (rfound == null)
            {
                await _roleManager.CreateAsync(new Role { Name = rolename, OrgId = "DOAN" });
            }
        }
        return Ok("Init success");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return Ok(result);
        return BadRequest(result.Message);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var result = await _mediator.Send(new GetMeQuery());
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken([FromBody] RefeshTokenQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }
}
