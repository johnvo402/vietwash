using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Services;
using Mediator;

namespace Application.Feature.Services.Command.Create
{
    public class CreateServiceHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IMediaUpdateService<Service> mediaUpdateService
    ) : IRequestHandler<CreateServiceCommand>
    {
        public async ValueTask<Mediator.Unit> Handle(
            CreateServiceCommand request,
            CancellationToken cancellationToken
        )
        {
            Service mappingService = mapper.Map<Service>(request);

            string? serviceImage = null;
            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(
                    cancellationToken
                );

                Service service = await unitOfWork
                    .Repository<Service>()
                    .AddAsync(mappingService, cancellationToken);
                serviceImage = service.Image;

                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);
                return Mediator.Unit.Value;
            }
            catch (Exception)
            {
                await mediaUpdateService.DeleteAvatarAsync(serviceImage);
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
