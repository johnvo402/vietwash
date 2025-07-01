using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using FluentValidation;

namespace Application.Feature.BranchProducts.Command.Delete
{
	public class DeleteBranchProductCommandValidator : AbstractValidator<DeleteBranchProductCommand>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;

		public DeleteBranchProductCommandValidator(
			IUnitOfWork unitOfWork,
			IActionAccessorService accessorService
		)
		{
			this.unitOfWork = unitOfWork;
			this.accessorService = accessorService;
			ApplyRules();
		}
		private void ApplyRules()
		{
			RuleFor(x => x.BranchProductId)
				.NotEmpty()
				.WithState(x =>
						Messager
							.Create<BranchProduct>()
							.Property(x => x.Id)
							.Message(MessageType.Null)
							.Negative()
							.Build()
				);

		}
	}
}
