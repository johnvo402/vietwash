using AuthService.Application.Commands;
using MediatR;
using Micro.Shared.Model;

namespace AuthService.Application.Queries;

public record RefeshTokenQuery(string RefreshToken) : IRequest<ApiResponse<LoginUserResponse>>;
