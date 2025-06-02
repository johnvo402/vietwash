using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Services;
using Infrastructure.UnitOfWorks;
using Mediator;
using Contracts.Utils;

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
            mappingService.Slug = Generator.GenerateSlug(mappingService.Name);
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
