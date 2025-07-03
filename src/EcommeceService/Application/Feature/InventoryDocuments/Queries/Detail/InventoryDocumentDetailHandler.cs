using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Spectifications;
using Mediator;

namespace Application.Feature.InventoryDocuments.Queries.Detail
{
    public class InventoryDocumentDetailHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<InventoryDocumentDetailQuery, Result<InventoryDocumentDetailResponse>>
    {
        public async ValueTask<Result<InventoryDocumentDetailResponse>> Handle(
            InventoryDocumentDetailQuery request,
            CancellationToken cancellationToken
        )
        {
            var result = await unitOfWork
                .DynamicReadOnlyRepository<InventoryDocument>()
                .FindByConditionAsync(
                    new GetInventoryDocumentByIdSpecification(request.Id),
                    o => o.ToInventoryDocumentDetailResponse(),
                    cancellationToken
                );
            if (result == null)
            {
                return Result<InventoryDocumentDetailResponse>.Failure(
                    new NotFoundError(
                        "Inventory document not found",
                        Messager
                            .Create<InventoryDocument>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }

            return Result<InventoryDocumentDetailResponse>.Success(result);
        }
    }
}
