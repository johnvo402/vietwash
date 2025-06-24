using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Utils;
using Domain.Aggregates.Services;
using Mediator;

namespace Application.Feature.Services.Command.Create
{
    public class CreateServiceHandler(
        IUnitOfWork unitOfWork,
        IMediaUpdateService<Service> mediaUpdateService
    ) : IRequestHandler<CreateServiceCommand, Result>
    {
        public async ValueTask<Result> Handle(
            CreateServiceCommand request,
            CancellationToken cancellationToken
        )
        {
            Service mappingService = request.ToEntity();
            mappingService.Slug = Generator.GenerateSlug(mappingService.Name);
            string? serviceImage = null;
            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                Service service = await unitOfWork
                    .Repository<Service>()
                    .AddAsync(mappingService, cancellationToken);
                serviceImage = service.Image;

                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception)
            {
                if (!string.IsNullOrEmpty(serviceImage))
                {
                    await mediaUpdateService.DeleteAvatarAsync(serviceImage);
                }
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
