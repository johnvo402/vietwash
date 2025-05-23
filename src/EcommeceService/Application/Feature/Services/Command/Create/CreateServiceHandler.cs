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

namespace Application.Feature.Services.Command.Create
{
    public class CreateServiceHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IMediaUpdateService<Service> mediaUpdateService,
        IActionAccessorService accessorService // Thêm để lấy currentUserId
	) : IRequestHandler<CreateServiceCommand>
    {
        public async ValueTask<Mediator.Unit> Handle(
            CreateServiceCommand request,
            CancellationToken cancellationToken

		)
        {
            Service mappingService = mapper.Map<Service>(request);
			mappingService.UnitRelations = new List<UnitRelation>();

			string? serviceImage = null;
            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(
                    cancellationToken
                );
                // Gán các giá trị bắt buộc cho Service
                var currentUserId = accessorService.Id;
				mappingService.CreatedBy = currentUserId;
				mappingService.UpdatedBy = currentUserId;
				mappingService.CreatedAt = DateTime.UtcNow;
				mappingService.UpdatedAt = DateTime.UtcNow;

				if (string.IsNullOrEmpty(mappingService.Slug))
				{
					mappingService.Slug = GenerateSlug(mappingService.Name);
					bool slugExists = await unitOfWork.Repository<Service>()
				.AnyAsync(s => s.Slug == mappingService.Slug, cancellationToken);
					if (slugExists)
					{
						throw new InvalidOperationException($"Slug '{mappingService.Slug}' already exists.");
					}
				}

				Service service = await unitOfWork
                    .Repository<Service>()
                    .AddAsync(mappingService, cancellationToken);
                serviceImage = service.Image;
				await unitOfWork.SaveAsync(cancellationToken);

				var unitRelations = mapper.Map<List<UnitRelation>>(request.UnitRelations);
				foreach (var unitRelation in unitRelations)
				{
					unitRelation.ReferenceId = service.Id;
					unitRelation.CreatedBy = currentUserId;
					unitRelation.UpdatedBy = currentUserId;
					unitRelation.CreatedAt = DateTime.UtcNow;
					unitRelation.UpdatedAt = DateTime.UtcNow;
					await unitOfWork.Repository<UnitRelation>().AddAsync(unitRelation, cancellationToken);
				}

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
		public string GenerateSlug(string input)
		{
			string slug = input.ToLowerInvariant()
				.Normalize(NormalizationForm.FormD);

			var sb = new StringBuilder();
			foreach (var c in slug)
				if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
					sb.Append(c);

			slug = Regex.Replace(sb.ToString(), @"[^a-z0-9\s-]", "");
			slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');

			return slug;
		}


	}
}
