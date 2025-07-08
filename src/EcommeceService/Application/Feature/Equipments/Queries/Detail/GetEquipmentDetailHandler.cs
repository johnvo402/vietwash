using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Specifications;
using Mediator;

namespace Application.Feature.Equipments.Queries.Detail
{
    public class GetEquipmentDetailHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetEquipmentDetailQuery, Result<GetEquipmentDetailResponse>>
    {
        public async ValueTask<Result<GetEquipmentDetailResponse>> Handle(
            GetEquipmentDetailQuery command,
            CancellationToken cancellationToken
        )
        {
            GetEquipmentDetailResponse? equipment = await unitOfWork
                .DynamicReadOnlyRepository<Equipment>()
                .FindByConditionAsync(
                    new GetEquipmentWithIncludeByIdSpecification(command.EquipmentId),
                    x => x.ToGetEquipmentDetailResponse(),
                    cancellationToken
                );
            if (equipment == null)
            {
                return Result<GetEquipmentDetailResponse>.Failure(
                    new NotFoundError(
                        "Equipment not found",
                        Messager
                            .Create<Equipment>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }

            return Result<GetEquipmentDetailResponse>.Success(equipment);
        }
    }
}
