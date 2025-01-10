
using AuthService.Application.Interfaces;
using AuthService.Domain.Users.Entity;
using AutoMapper;
using ErrorOr;
using MediatR;
using Micro.Shared.Infrastructure.CurrentUserProvider;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Users.Commands.UpdateUserCommands;
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ErrorOr<string>>
{
    private readonly IUserRepo _userRepo;
    private readonly IPasswordHasher<User> _passwordHash;
    private readonly IRoleRepo _roleRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(IUserRepo userRepo,
     IPasswordHasher<User> passwordHash,
      IRoleRepo roleRepo, ICurrentUser currentUser,
       IMapper mapper)
    {
        _userRepo = userRepo;
        _passwordHash = passwordHash;
        _roleRepo = roleRepo;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<ErrorOr<string>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIDAsync(request.request.Id);
        if (user == null)
        {
            return Error.NotFound("backend.users.notfound");
        }
        _mapper.Map(request.request.Object, user);
        if (!string.IsNullOrEmpty(request?.request?.Object?.Password))
        {
            user.Password = _passwordHash.HashPassword(user, request.request.Object.Password);
        }
        bool checkUpdate = await _userRepo.UpdateAsync(user, cancellationToken);
        if (!checkUpdate)
        {
            return Error.Failure("backend.users.update.failed");
        }
        return "backend.users.update.success";
    }

}
