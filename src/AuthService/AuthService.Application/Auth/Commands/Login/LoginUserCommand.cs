using ErrorOr;
using MediatR;
using Micro.Shared.Application.Security.Request;
namespace AuthService.Application.Auth.Commands.Login;

public record LoginUserCommand(string Email, string Password) : IRequest<ErrorOr<LoginUserResponse>>;
public class LoginUserResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? ExpiresAt { get; set; }
}