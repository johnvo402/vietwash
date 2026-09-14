using Application.Feature.AiAssistant.Models;

namespace Application.Feature.AiAssistant.Interfaces;

public interface IAiChatClient
{
    Task<AiChatResult> CompleteAsync(
        AiChatRequest request,
        CancellationToken cancellationToken
    );
}
