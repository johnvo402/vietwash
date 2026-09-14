using Contracts.ApiWrapper;
using Microsoft.AspNetCore.Http;

namespace Application.Feature.AiAssistant.Models;

public sealed class AiAssistantUnavailableError(string detail)
    : ErrorDetails(
        "AI assistant is unavailable",
        detail,
        nameof(AiAssistantUnavailableError),
        StatusCodes.Status503ServiceUnavailable
    );

public sealed class AiAssistantProviderError(string detail)
    : ErrorDetails(
        "AI provider returned an invalid response",
        detail,
        nameof(AiAssistantProviderError),
        StatusCodes.Status502BadGateway
    );
