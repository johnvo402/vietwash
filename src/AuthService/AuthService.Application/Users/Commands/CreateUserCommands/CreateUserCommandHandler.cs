
using AuthService.Application.Auth.Commands.CreateUserCommands;
using AuthService.Application.Interfaces;
using AuthService.Domain.Roles;
using AuthService.Domain.UserRoles;
using AuthService.Domain.Users.Entity;
using ErrorOr;
using MediatR;
using Micro.Shared.Data;
using Micro.Shared.Infrastructure.CurrentUserProvider;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auth.Commands.CreateUserCommands;
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ErrorOr<string>>
{
    private readonly IUserRepo _userRepo;
    private readonly IPasswordHasher<User> _passwordHash;
    private readonly IRoleRepo _roleRepo;
    private readonly ICurrentUser _currentUser;

    public CreateUserCommandHandler(IUserRepo userRepo, IPasswordHasher<User> passwordHash, IRoleRepo roleRepo, ICurrentUser currentUser)
    {
        _userRepo = userRepo;
        _passwordHash = passwordHash;
        _roleRepo = roleRepo;
        _currentUser = currentUser;
    }

    public async Task<ErrorOr<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if all roles exist in the database
        var rolesExist = (await _roleRepo.GetAllAsync(new QueryParameters
        {
            Where = $"role_name in ({string.Join(",", request.Role.Select(x => $"'{x}'"))})"
        }));
        if (!rolesExist.Count().Equals(request.Role.Count))
        {
            return Error.NotFound(description: "backend.auth.role_not_found");
        }

        // Check if the user already exists (optional but important)
        var existingUser = await _userRepo.GetUserByEmail(request.Email, cancellationToken);
        if (existingUser != null)
        {
            return Error.Validation(description: "backend.auth.email_already_exists");
        }

        // Create the user object
        var user = new User
        {
            DisplayName = request.DisplayName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            OrgId = "DOAN",
        };

        // Hash the user's password
        user.Password = _passwordHash.HashPassword(user, request.Password);

        // Create the new user in the database
        var newUser = await _userRepo.CreateAsync(user, cancellationToken);

        // Assign roles to the user
        var addRole = await _roleRepo.AddRoleToUser(newUser.Id, request.Role, cancellationToken);
        if (!addRole)
        {
            return Error.Failure(description: "backend.auth.register_failed");
        }

        // Return the user ID as a string
        return newUser.Id.ToString();
    }

}
