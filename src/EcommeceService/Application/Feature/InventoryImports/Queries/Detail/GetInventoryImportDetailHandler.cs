using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using Domain.Aggregates.Inventories;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Domain.Aggregates.Inventories.Specifications;

namespace Application.Feature.InventoryImports.Queries.Detail
{
    public class GetInventoryImportDetailHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetInventoryImportDetailQuery, GetInventoryImportDetailResponse>
    {
        public async ValueTask<GetInventoryImportDetailResponse> Handle(
            GetInventoryImportDetailQuery query,
            CancellationToken cancellationToken
        )
        {

            var inventoryImport =
                await unitOfWork
                    .Repository<InventoryDocument>()
                    .FindByConditionAsync(
                        new GetInventoryImportWithIncludeByIdSpecification(query.inventoryImportId),
                        cancellationToken
                    )
                ?? throw new NotFoundException(
                    [Messager.Create<InventoryDocument>().Message(MessageType.Found).Negative().BuildMessage()]
                );


            var response = mapper.Map<GetInventoryImportDetailResponse>(inventoryImport);



            return response;
        }
    }
}
