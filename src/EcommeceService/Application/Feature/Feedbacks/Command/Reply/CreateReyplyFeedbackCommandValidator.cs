using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Feedbacks;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using FluentValidation;

namespace Application.Feature.Feedbacks.Command.Reply
{
    public class CreateReyplyFeedbackCommandValidator
        : AbstractValidator<CreateReyplyFeedbackCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentAccount currentCustomer;

        public CreateReyplyFeedbackCommandValidator(
            IUnitOfWork unitOfWork,
            ICurrentAccount currentCustomer
        )
        {
            _unitOfWork = unitOfWork;
            this.currentCustomer = currentCustomer;
            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => currentCustomer.Id)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Feedback>()
                        .Property(x => x.User)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                );
            RuleFor(x => x.ReplyFeedback.Comment)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<ReplyFeedbackModel>(nameof(Feedback))
                        .Property(x => x.Comment)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MaximumLength(500)
                .WithState(x =>
                    Messager
                        .Create<ReplyFeedbackModel>(nameof(Feedback))
                        .Property(x => x.Comment)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<CreateReyplyFeedbackCommand>(nameof(Feedback))
                        .Property(x => x.Id)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MustAsync(IsFeedbackExistsAsync)
                .WithState(x =>
                    Messager
                        .Create<CreateReyplyFeedbackCommand>(nameof(Feedback))
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
                .AnyAsync(p => p.Id == feedbackId, cancellation);
        }
    }
}
