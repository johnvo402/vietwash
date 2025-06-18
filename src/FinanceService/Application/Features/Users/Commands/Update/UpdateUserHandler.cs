using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediaUpdateService<User> mediaUpdateService
) : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async ValueTask<UpdateUserResponse> Handle(
        UpdateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        User user =
            await unitOfWork
                .Repository<User>()
                .FindByConditionAsync(
                    new GetUserByIdWithoutIncludeSpecification(command.UserId),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<User>().Message(MessageType.Found).Negative().BuildMessage()]
            );

        string? oldAvatar = user.AvtUrl;

        mapper.Map(command.User, user);

        // update default claim

        try
        {
            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            await unitOfWork.Repository<User>().UpdateAsync(user);
            await unitOfWork.SaveAsync(cancellationToken);

            await unitOfWork.Repository<User>().UpdateAsync(user);
            await unitOfWork.CommitAsync(cancellationToken);

            await mediaUpdateService.DeleteAvatarAsync(oldAvatar);
            return mapper.Map<UpdateUserResponse>(user);
        }
        catch (Exception)
        {
            await mediaUpdateService.DeleteAvatarAsync(user.AvtUrl);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
