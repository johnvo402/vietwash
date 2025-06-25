using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Products;
using FluentValidation;


namespace Application.Feature.Products.Command.Create
{
	public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		private readonly IActionAccessorService _accessorService;

		public CreateProductCommandValidator(
			IUnitOfWork unitOfWork,
			IActionAccessorService accessorService
		)
		{
			_unitOfWork = unitOfWork;
			_accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			Include(new ProductValidator(_unitOfWork, _accessorService));
			
		}
	}
}
