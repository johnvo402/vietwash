using Application.Feature.AiAssistant.Models;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.AiAssistant.Commands.Chat;

public sealed class AiChatCommand : IRequest<Result<AiChatResponse>>
{
    public string Message { get; init; } = string.Empty;
    public string? ConversationId { get; init; }
}
