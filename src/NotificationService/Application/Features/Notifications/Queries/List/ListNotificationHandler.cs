using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Notifications;
using Domain.Aggregates.Notifications.Spectifications;
using Mediator;

namespace Application.Features.Notifications.Queries.List
{
    public class ListNotificationHandler(IUnitOfWork unitOfWork, ICurrentAccount current)
        : IRequestHandler<
            ListNotificationQuery,
            Result<PaginationResponse<ListNotificationResponse>>
        >
    {
        public async ValueTask<Result<PaginationResponse<ListNotificationResponse>>> Handle(
            ListNotificationQuery request,
            CancellationToken cancellationToken
        )
        {
            var validation = request.Validate<ListNotificationQuery, ListNotificationResponse>();

            if (validation != null)
            {
                return validation;
            }
            var id = current.Id.ToString();
            if (string.IsNullOrEmpty(id))
            {
                return Result<PaginationResponse<ListNotificationResponse>>.Failure(
                    new BadRequestError(
                        "Error has occured with notification",
                        Messager
                            .Create<ListNotificationQuery>(nameof(Notification))
                            .Message(MessageType.Valid)
                            .Negative()
                            .Build()
                    )
                );
            }
            var notifications = await unitOfWork
                .DynamicReadOnlyRepository<Notification>()
                .PagedListAsync(
                    new GetNotificationByUserIdSpecification(id),
                    request,
                    ListNotificationMapping.Selector(),
                    cancellationToken
                );
            return Result<PaginationResponse<ListNotificationResponse>>.Success(notifications);
        }
    }
}
