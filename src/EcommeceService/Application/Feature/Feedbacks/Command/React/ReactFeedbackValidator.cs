using Application.Feature.Common.Projections.Feedbacks;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Feedbacks.Enums;
using FluentValidation;

namespace Application.Feature.Feedbacks.Command.React
{
    public class ReactFeedbackValidator : AbstractValidator<ReactFeedbackCommand>
    {
        public ReactFeedbackValidator()
        {
            RuleFor(x => x.FeedbackReaction.ReactionType)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<FeedbackReactionModel>(nameof(Feedback))
                        .Property(x => x.ReactionType!)
                        .Message(MessageType.Empty)
                        .Negative()
                        .Build()
                )
                .IsInEnum()
                .WithState(x =>
                    Messager
                        .Create<FeedbackReactionModel>(nameof(Feedback))
                        .Property(x => x.ReactionType!)
                        .Message(MessageType.Correct)
                        .Negative()
                        .Build()
                )
                .Must(value => Enum.IsDefined(typeof(ReactionType), value))
                .WithState(x =>
                    Messager
                        .Create<FeedbackReactionModel>(nameof(Feedback))
                        .Property(x => x.ReactionType!)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build()
                );
        }
    }
}
