using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Regions;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;

namespace Application.Features.Users.Commands.Create;

public class CreateUserHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediaUpdateService<User> mediaUpdateService
) : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    public async ValueTask<CreateUserResponse> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        User mappingUser = mapper.Map<User>(command);

        Province? province = await unitOfWork
            .Repository<Province>()
            .FindByIdAsync(Ulid.Parse(command.ProvinceId), cancellationToken);
        District? district = await unitOfWork
            .Repository<District>()
            .FindByIdAsync(Ulid.Parse(command.DistrictId), cancellationToken);

        Commune? commune = null;
        if (!string.IsNullOrEmpty(command.CommuneId))
        {
            commune = await unitOfWork
                .Repository<Commune>()
                .FindByIdAsync(Ulid.Parse(command.CommuneId), cancellationToken);
        }

        mappingUser.UpdateAddress(new(province!, district!, commune, command.Street!));

        string? key = mediaUpdateService.GetKey(command.Avatar);
        mappingUser.Avatar = await mediaUpdateService.UploadAvatarAsync(command.Avatar, key);

        string? userAvatar = null;
        try
        {
            
            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            User user = await unitOfWork
                .Repository<User>()
                .AddAsync(mappingUser, cancellationToken);
            userAvatar = user.Avatar;

            await unitOfWork.SaveAsync(cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);

            return (
                await unitOfWork
                    .Repository<User>()
                    .FindByConditionAsync<CreateUserResponse>(
                        new GetUserByIdSpecification(user.Id),
                        cancellationToken
                    )
            )!;
        }
        catch (Exception)
        {
            await mediaUpdateService.DeleteAvatarAsync(userAvatar);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
