using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Units;
using Application.Feature.Common.Validators.Services;
using Domain.Aggregates.Services;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Services.Command.Create
{
	public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IActionAccessorService _accessorService;

		public CreateServiceCommandValidator(
			IUnitOfWork unitOfWork,
			IActionAccessorService accessorService)
		{
			_unitOfWork = unitOfWork;
			_accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			// Bao gồm các quy tắc từ ServiceValidator
			Include(new ServiceValidator(_unitOfWork, _accessorService));

			// Quy tắc cho CategoryId (được kế thừa từ ServiceModel)
			RuleFor(x => x.CategoryId)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<Service>()
						.Property(x => x.CategoryId)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				)
				.MustAsync(IsCategoryAvailableAsync)
				.WithState(x =>
					Messager
						.Create<Service>()
						.Property(x => x.CategoryId)
						.Message(MessageType.Existence)
						.Negative()
						.Build()
				)
				.WithMessage("CategoryId does not exist.");

			// Quy tắc cho UnitRelation (được kế thừa từ ServiceModel)
			RuleFor(x => x.UnitRelation)
				.Must(HasValidUnitRelations)
				.When(x => x.UnitRelation != null && x.UnitRelation.Any())
				.WithState(x =>
					Messager
						.Create<CreateServiceCommand>()
						.Property(x => x.UnitRelation)
						.Message(MessageType.Valid)
						.Negative()
						.Build()
				)
				.WithMessage("UnitRelation contains invalid data.");

			// Quy tắc cho Image (được kế thừa từ ServiceModel)
			RuleFor(x => x.Image)
				.Must(BeAValidImage)
				.When(x => x.Image != null)
				.WithState(x =>
					Messager
						.Create<Service>()
						.Property(x => x.Image)
						.Message(MessageType.Valid)
						.Negative()
						.Build()
				)
				.WithMessage("Image must be a valid file (e.g., jpg, png).");
		}
		private async Task<bool> IsCategoryAvailableAsync(Ulid categoryId, CancellationToken cancellationToken)
		{
			return await _unitOfWork.Repository<Category>().AnyAsync(x => x.Id == categoryId, cancellationToken);
		}

		private bool HasValidUnitRelations(List<UnitRelationModel> unitRelations)
		{
			if (unitRelations == null || !unitRelations.Any()) return true; // Cho phép rỗng
			return unitRelations.All(ur => ur.Id != Ulid.Empty);
		}

		private bool BeAValidImage(IFormFile image)
		{
			if (image == null) return true; // Cho phép null
			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
			var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
			return allowedExtensions.Contains(fileExtension) && image.Length > 0;
		}
	}
		

}
