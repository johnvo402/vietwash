
using AuthService.Application.Interfaces;
using AuthService.Domain.Roles;
using AuthService.Domain.Users.Entity;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Auth.Commands.Login;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ErrorOr<LoginUserResponse>>
{
    private readonly IUserRepo _userRepo;
    private readonly ITokenHelper _tokenHelper;
    private readonly IPasswordHasher<User> _passwordHash;
    private readonly IRoleRepo _roleRepo;
    private readonly IPermissionRepo _permissionRepo;

    public LoginUserCommandHandler(IUserRepo userRepo, IPasswordHasher<User> passwordHash, ITokenHelper tokenHelper, IRoleRepo roleRepo, IPermissionRepo permissionRepo)
    {
        _userRepo = userRepo;
        _passwordHash = passwordHash;
        _tokenHelper = tokenHelper;
        _roleRepo = roleRepo;
        _permissionRepo = permissionRepo;
    }

    public async Task<ErrorOr<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetUserByEmail(request.Email, cancellationToken);
        if (user == null)
        {
            return Error.NotFound(description: "backend.notfound.email");
        }

        var verify = _passwordHash.VerifyHashedPassword(user, user.Password, request.Password);
        if (verify != PasswordVerificationResult.Success)
        {
            return Error.Validation(description: "backend.auth.login_failed");
        }
        // user.UpdateLastLogin();


        var roles = _roleRepo.GetRolesByUserId(user.Id, cancellationToken).Result.ToList();

        var permissions = _permissionRepo.GetPermissionsByRoleIds(roles.Select(s => s.Id).ToList(), cancellationToken).Result.Select(x => x.PermissionKey).ToList();

        var (accessToken, exp) = _tokenHelper.GenerateAccessToken(user.Id.ToString(), user.DisplayName ?? "", user.Email, permissions, roles.Select(x => x.RoleName).ToList(), user.OrgId ?? "DOAN");
        var refreshToken = _tokenHelper.GenerateRefreshToken(user.Id.ToString());
        _userRepo.Update(user);
        return new LoginUserResponse
        {
            AccessToken = accessToken,
            ExpiresAt = exp,
            RefreshToken = refreshToken
        };
    }
}

