using Micro.Shared.Data;
using ErrorOr;
using Micro.Shared.Application.Security.Request;
using MediatR;
namespace AuthService.Application.Auth.Commands.CreateUserCommands;
// [Authorize(Roles = "Admin, Manager", Permissions = "user.create")]
public record CreateUserCommand(
    string Password,
    List<string> Role,
    string Email = null!,
    string? DisplayName = null,
    string? PhoneNumber = null
) : IAuthorizeableRequest<ErrorOr<string>>;
