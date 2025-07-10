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

		public CreateFeedbackCommandValidator(
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
			Include(new FeedbackValidator(_unitOfWork, _accessorService));
		}
	}
}
