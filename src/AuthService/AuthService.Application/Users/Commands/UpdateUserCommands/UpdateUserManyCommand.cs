using ErrorOr;
using Micro.Shared.Application.Security.Request;
using Micro.Shared.Model;

namespace AuthService.Application.Users.Commands.UpdateUserCommands;

public record UpdateUserManyCommand(IEnumerable<ApiRequestPut<UserUpdateDto>> request) : IAuthorizeableRequest<ErrorOr<string>>;