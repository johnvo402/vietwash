using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Mapping.Users;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Services.Queries.Detail;

public class GetServiceDetailHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetServiceDetailQuery, Result<GetServiceDetailResponse>>
{
    public async ValueTask<Result<GetServiceDetailResponse>> Handle(
        GetServiceDetailQuery command,
        CancellationToken cancellationToken
    )
    {
        GetServiceDetailResponse? service = await unitOfWork
            .DynamicReadOnlyRepository<Service>()
            .FindByConditionAsync(
                new GetServiceWithIncludeByIdSpecification(command.ServiceId),
                x => x.GetDetailSelector(),
                cancellationToken
            );
        if (service == null)
        {
            return Result<GetServiceDetailResponse>.Failure(
                new NotFoundError(
                    "Service not found",
                    Messager.Create<Service>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }
        if (!string.IsNullOrEmpty(service.CreatedBy) && service.CreatedBy != "SYSTEM")
        {
            UserDTO? createdUser = await unitOfWork
                .DynamicReadOnlyRepository<User>()
                .FindByConditionAsync(
                    new GetUserByIdWithoutIncludeSpecification(long.Parse(service.CreatedBy)),
                    x => x.UserDTOResponse(),
                    cancellationToken
                );

            service.CreatedUser = createdUser;
        }
        if (!string.IsNullOrEmpty(service.UpdatedBy) && service.UpdatedBy != "SYSTEM")
        {
            UserDTO? updatedUser = await unitOfWork
                .DynamicReadOnlyRepository<User>()
                .FindByConditionAsync(
                    new GetUserByIdWithoutIncludeSpecification(long.Parse(service.UpdatedBy)),
                    x => x.UserDTOResponse(),
                    cancellationToken
                );

            service.UpdatedUser = updatedUser;
        }

		var averageRating = await unitOfWork
	        .Repository<Feedback>()
	        .QueryAsync()
	        .Where(x => x.ServiceId == service.Id && !x.Disable && x.Rating != null)
	        .AverageAsync(x => (double?)x.Rating, cancellationToken) ?? 0;

        service.AverageRating = averageRating;

		return Result<GetServiceDetailResponse>.Success(service);
    }
}
