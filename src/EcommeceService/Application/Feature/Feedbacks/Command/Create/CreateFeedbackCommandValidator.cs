using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Feedbacks;
using FluentValidation;

namespace Application.Feature.Feedbacks.Command.Create
{
    public class CreateFeedbackCommandValidator : AbstractValidator<CreateFeedbackCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentAccount _currentCustomer;

        IActionAccessorService _accessorService;

        public CreateFeedbackCommandValidator(
            IUnitOfWork unitOfWork,
            ICurrentAccount currentCustomer,
            IActionAccessorService accessorService
        )
        {
            _accessorService = accessorService;
            _unitOfWork = unitOfWork;
            _currentCustomer = currentCustomer;
            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.FeedbackModel)
                .SetValidator(
                    new FeedbackValidator(_unitOfWork, _currentCustomer, _accessorService)
                );
        }
    }
}
