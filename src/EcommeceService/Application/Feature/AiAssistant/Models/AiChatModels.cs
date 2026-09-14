using System.Text.Json;

namespace Application.Feature.AiAssistant.Models;

public enum AiChatRole
{
    User,
    Assistant,
    Tool,
}

public sealed record AiToolDefinition(
    string Name,
    string Description,
    JsonElement InputSchema
);

public sealed record AiToolCall(
    string? Id,
    string Name,
    JsonElement Arguments,
    string? ProviderMetadata = null
);

public sealed record AiToolResult(string? CallId, string Name, JsonElement Result);

public sealed record AiChatMessage(
    AiChatRole Role,
    string? Text = null,
    IReadOnlyList<AiToolCall>? ToolCalls = null,
    IReadOnlyList<AiToolResult>? ToolResults = null
);

public sealed record AiChatRequest(
    string SystemInstruction,
    IReadOnlyList<AiChatMessage> Messages,
    IReadOnlyList<AiToolDefinition> Tools
);

public sealed record AiTokenUsage(int? InputTokens, int? OutputTokens, int? TotalTokens);

public sealed record AiChatResult(
    string? Answer,
    IReadOnlyList<AiToolCall> ToolCalls,
    AiTokenUsage? TokenUsage = null
);

public sealed class AiChatResponse
{
    public required string Answer { get; init; }
    public required string ConversationId { get; init; }
    public IReadOnlyList<string> ToolsUsed { get; init; } = [];
}
