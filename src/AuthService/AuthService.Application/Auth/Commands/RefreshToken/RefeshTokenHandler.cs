using AuthService.Application.Auth.Commands.Login;
using AuthService.Application.Interfaces;
using ErrorOr;
using MediatR;
using Micro.Shared.Infrastructure.CurrentUserProvider;

namespace AuthService.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Handles the refresh token command to generate new access and refresh tokens
/// </summary>
/// <remarks>
/// This handler validates the refresh token and generates new tokens for authenticated users
/// </remarks>
/// <seealso cref="IRequestHandler{RefeshTokenCommand, ErrorOr{LoginUserResponse}}"/>
public class RefeshTokenHandler : IRequestHandler<RefeshTokenCommand, ErrorOr<LoginUserResponse>>
{
    private readonly ITokenHelper _tokenHelper;
    private readonly IUserRepo _userRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IRoleRepo _roleRepo;
    private readonly IPermissionRepo _permissionRepo;

    public RefeshTokenHandler(ITokenHelper tokenHelper, IUserRepo userRepo, ICurrentUser currentUser, IRoleRepo roleRepo, IPermissionRepo permissionRepo)
    {
        _tokenHelper = tokenHelper;
        _userRepo = userRepo;
        _currentUser = currentUser;
        _roleRepo = roleRepo;
        _permissionRepo = permissionRepo;
    }



    Task<ErrorOr<LoginUserResponse>> IRequestHandler<RefeshTokenCommand, ErrorOr<LoginUserResponse>>.Handle(RefeshTokenCommand request, CancellationToken cancellationToken)
    {
        var check = _tokenHelper.ValidateAccessToken(request.RefreshToken);
        if (!check)
        {
            return Task.FromResult<ErrorOr<LoginUserResponse>>(Error.Validation("backend.auth.invalid_token"));
        }

        var user = _userRepo.GetByID(Guid.Parse(_currentUser.Id));
        if (user == null)
        {
            return Task.FromResult<ErrorOr<LoginUserResponse>>(Error.NotFound("backend.user.notfound"));
        }
        var roles = _roleRepo.GetRolesByUserId(user.Id, cancellationToken).Result.ToList();

        var permissions = _permissionRepo.GetPermissionsByRoleIds(roles.Select(s => s.Id).ToList(), cancellationToken).Result.Select(x => x.PermissionKey).ToList();

        var (accessToken, exp) = _tokenHelper.GenerateAccessToken(user.Id.ToString(), user.DisplayName ?? "", user.Email, permissions, roles.Select(x => x.RoleName).ToList(), user.OrgId ?? "DOAN");
        var refreshToken = _tokenHelper.GenerateRefreshToken();
        var response = new LoginUserResponse
        {
            AccessToken = accessToken,
            ExpiresAt = exp,
            RefreshToken = refreshToken
        };
        return Task.FromResult<ErrorOr<LoginUserResponse>>(response);
    }
}