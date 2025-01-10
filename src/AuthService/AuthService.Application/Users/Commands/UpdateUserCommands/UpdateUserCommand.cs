using AuthService.Domain.Users.Entity;
using ErrorOr;
using Micro.Shared.Application.Security.Request;
using Micro.Shared.Model;

namespace AuthService.Application.Users.Commands.UpdateUserCommands;

public record UpdateUserCommand(ApiRequestPut<UserUpdateDto> request) : IAuthorizeableRequest<ErrorOr<string>>;

public record UserUpdateDto
{
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public string? Password { get; set; }
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
}