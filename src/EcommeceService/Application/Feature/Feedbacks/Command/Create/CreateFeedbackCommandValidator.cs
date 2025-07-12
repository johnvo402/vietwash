using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Feedbacks;
using FluentValidation;

namespace Application.Feature.Feedbacks.Command.Create
{
	public class CreateFeedbackCommandValidator : AbstractValidator<CreateFeedbackCommand>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IActionAccessorService _accessorService;
		private readonly ICurrentAccount _currentCustomer;

		public CreateFeedbackCommandValidator(
			IUnitOfWork unitOfWork,
			IActionAccessorService accessorService,
			ICurrentAccount currentCustomer
		)
		{
			_unitOfWork = unitOfWork;
			_accessorService = accessorService;
			_currentCustomer = currentCustomer;
			ApplyRules();
		}

		private void ApplyRules()
		{
			Include(new FeedbackValidator(_unitOfWork, _accessorService, _currentCustomer));
		}
	}
}
