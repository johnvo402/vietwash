using System.Text.Json.Serialization;
using MediatR;
using Micro.Shared.Model;
namespace AuthService.Application.Commands;

public record LoginUserCommand(string Email, string Password) : IRequest<ApiResponse<LoginUserResponse>>;
public class LoginUserResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
}