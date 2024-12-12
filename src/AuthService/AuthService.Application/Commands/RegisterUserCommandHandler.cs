using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using MediatR;
using Micro.Shared.Data;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Commands;
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ApiResponse<string>>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    public RegisterUserCommandHandler(UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ApiResponse<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = result.Errors.FirstOrDefault()?.Description ?? "User creation failed",
            };
        }
        await _userManager.AddToRoleAsync(user, request.Role);
        return new ApiResponse<string>
        {
            Success = true,
            Value = user.Id,
        };
    }
}
