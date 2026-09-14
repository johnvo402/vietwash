using System.Text.Json;
using Application.Feature.AiAssistant.Models;

namespace Application.Feature.AiAssistant.Interfaces;

public interface IAiBusinessToolRegistry
{
    IReadOnlyList<AiToolDefinition> Definitions { get; }

    Task<JsonElement> ExecuteAsync(
        string toolName,
        JsonElement arguments,
        AiToolExecutionContext context,
        CancellationToken cancellationToken
    );
}

public sealed record AiToolExecutionContext(
    IReadOnlySet<long> AuthorizedBranchIds,
    TimeZoneInfo TimeZone
);
