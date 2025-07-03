using Application.Feature.Common.Projections.Inventories;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.InventoryDocuments.Commands.Create
{
    public class CreateInventoryDocumentCommand
        : InventoryDocumentModel,
            IRequest<Result<CreateInventoryDocumentResponse>>;
}
