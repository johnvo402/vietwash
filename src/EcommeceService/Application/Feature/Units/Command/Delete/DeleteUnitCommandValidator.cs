using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Services;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Units.Command.Delete
{
	public class DeleteUnitCommandValidator : AbstractValidator<DeleteUnitCommand>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IActionAccessorService _accessorService;

		public DeleteUnitCommandValidator(
			IUnitOfWork unitOfWork,
			IActionAccessorService accessorService)
		{
			_unitOfWork = unitOfWork;
			_accessorService = accessorService;

			ApplyRules();
		}

		private void ApplyRules()
		{
			// Quy tắc cho UnitId
			RuleFor(x => x.UnitId)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<Unit>()
						.Property(x => x.Id)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				)
				.WithMessage("UnitId cannot be empty.")
				.MustAsync(IsUnitExistsAsync)
				.WithState(x =>
					Messager
						.Create<Unit>()
						.Property(x => x.Id)
						.Message(MessageType.Existence)
						.Negative()
						.Build()
				)
				.WithMessage("Unit with the specified UnitId does not exist.");
		}

		private async Task<bool> IsUnitExistsAsync(Ulid unitId, CancellationToken cancellationToken)
		{
			return await _unitOfWork
				.Repository<Unit>()
				.AnyAsync(u => u.Id == unitId, cancellationToken);
		}
	}
}
