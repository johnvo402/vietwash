using AuthService.Application.Auth.Commands.Login;
using ErrorOr;
using MediatR;
using Micro.Shared.Model;

namespace AuthService.Application.Auth.Commands.RefreshToken;

public record RefeshTokenCommand(string RefreshToken) : IRequest<ErrorOr<LoginUserResponse>>;
