using FluentValidation;

namespace Application.Feature.AiAssistant.Commands.Chat;

public sealed class AiChatCommandValidator : AbstractValidator<AiChatCommand>
{
    public AiChatCommandValidator()
    {
        RuleFor(command => command.Message).NotEmpty().MaximumLength(2_000);
        RuleFor(command => command.ConversationId)
            .Must(value => value is null || Guid.TryParse(value, out _))
            .WithMessage("conversationId must be a UUID.");
    }
}
