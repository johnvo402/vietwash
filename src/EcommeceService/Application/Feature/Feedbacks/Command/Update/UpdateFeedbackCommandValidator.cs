using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Feedbacks;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Feedbacks.Command.Update
{
    public class UpdateFeedbackCommandValidator : AbstractValidator<UpdateFeedbackCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentAccount _currentCustomer;
        IActionAccessorService _accessorService;

        public UpdateFeedbackCommandValidator(
            IUnitOfWork unitOfWork,
            IActionAccessorService accessorService,
            ICurrentAccount currentCustomer
        )
        {
            _accessorService = accessorService;
            _unitOfWork = unitOfWork;
            _currentCustomer = currentCustomer;
            ApplyRules();
        }

        private void ApplyRules()
        {
            _ = long.TryParse(_accessorService.Id, out long id);
            RuleFor(x => x.FeedbackId)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<UpdateFeedbackCommand>(nameof(Feedback))
                        .Property(x => x.FeedbackId)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MustAsync(IsFeedbackExistsAsync)
                .WithState(x =>
                    Messager
                        .Create<Feedback>()
                        .Property(x => x.Id)
                        .Message(MessageType.Found)
                        .Negative()
                        .Build()
                );
        }

        private async Task<bool> IsFeedbackExistsAsync(
            long feedbackId,
            CancellationToken cancellation
        )
        {
            return await _unitOfWork
                .Repository<Feedback>()
                .AnyAsync(x => x.Id == feedbackId && !x.Disable, cancellation);
        }
    }
}
