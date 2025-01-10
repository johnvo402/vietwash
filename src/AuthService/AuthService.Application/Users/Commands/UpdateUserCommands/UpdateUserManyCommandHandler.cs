
using AuthService.Application.Interfaces;
using AuthService.Domain.Users.Entity;
using AutoMapper;
using ErrorOr;
using MediatR;
using Micro.Shared.Infrastructure.CurrentUserProvider;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Users.Commands.UpdateUserCommands;
public class UpdateUserManyCommandHandler : IRequestHandler<UpdateUserManyCommand, ErrorOr<string>>
{
    private readonly IUserRepo _userRepo;
    private readonly IPasswordHasher<User> _passwordHash;
    private readonly IRoleRepo _roleRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public UpdateUserManyCommandHandler(IUserRepo userRepo,
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

    public async Task<ErrorOr<string>> Handle(UpdateUserManyCommand requests, CancellationToken cancellationToken)
    {
        var param = new QueryParameters();
        var ids = string.Join(",", requests.request.Select(x => $"'{x.Id}'"));
        param.Where = $"Id IN ({ids})";
        var users = await _userRepo.GetAllAsync(param);
        if (users == null)
        {
            return Error.NotFound("backend.users.notfound");
        }
        if (users.Count() != requests.request.Count())
        {
            return Error.NotFound("backend.users.somenotfounds");
        }
        foreach (var request in requests.request)
        {
            var user = users.FirstOrDefault(u => u.Id == request.Id);
            if (user != null && request.Object != null)
            {
                _mapper.Map(request.Object, user);
                if (!string.IsNullOrEmpty(request.Object.Password))
                {
                    user.Password = _passwordHash.HashPassword(user, request.Object.Password);
                }
            }
        }

        int checkUpdate = await _userRepo.BulkUpdateAsync(users, cancellationToken);

        return "affected rows: " + checkUpdate;
    }

}
