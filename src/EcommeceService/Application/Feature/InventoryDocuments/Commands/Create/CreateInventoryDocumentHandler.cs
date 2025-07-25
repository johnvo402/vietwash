using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Models;
using Domain.Aggregates.Inventories;
using Mediator;

namespace Application.Feature.InventoryDocuments.Commands.Create
{
    public class CreateInventoryDocumentHandler(
        IUnitOfWork unitOfWork,
        IMediaUpdateService mediaUpdateService
    ) : IRequestHandler<CreateInventoryDocumentCommand, Result<CreateInventoryDocumentResponse>>
    {
        public async ValueTask<Result<CreateInventoryDocumentResponse>> Handle(
            CreateInventoryDocumentCommand request,
            CancellationToken cancellationToken
        )
        {
            var inventoryDocument = request.ToEntity();
            inventoryDocument.TransactionAt = request.TransactionAt;
            var uploadedImages = new List<string>();

            try
            {
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
                var response = await unitOfWork
                    .Repository<InventoryDocument>()
                    .AddAsync(inventoryDocument);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                return Result<CreateInventoryDocumentResponse>.Success(
                    new CreateInventoryDocumentResponse { Id = response.Id }
                );
            }
            catch (Exception)
            {
                foreach (var image in uploadedImages)
                {
                    await mediaUpdateService.DeleteMediaAsync(image);
                }
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
