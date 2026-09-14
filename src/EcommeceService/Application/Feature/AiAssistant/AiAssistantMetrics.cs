using System.Diagnostics;
using System.Diagnostics.Metrics;
using Application.Feature.AiAssistant.Models;

namespace Application.Feature.AiAssistant;

public sealed class AiAssistantMetrics : IDisposable
{
    public const string MeterName = "VietWash.AiAssistant";

    private readonly Meter meter = new(MeterName);
    private readonly Counter<long> requests;
    private readonly Counter<long> toolInvocations;
    private readonly Histogram<double> providerLatency;
    private readonly Histogram<double> toolLatency;
    private readonly Histogram<int> toolRounds;
    private readonly Counter<long> tokens;

    public AiAssistantMetrics()
    {
        requests = meter.CreateCounter<long>("vietwash.ai.requests", "{request}");
        toolInvocations = meter.CreateCounter<long>("vietwash.ai.tool.invocations", "{call}");
        providerLatency = meter.CreateHistogram<double>("vietwash.ai.provider.duration", "ms");
        toolLatency = meter.CreateHistogram<double>("vietwash.ai.tool.duration", "ms");
        toolRounds = meter.CreateHistogram<int>("vietwash.ai.tool.rounds", "{round}");
        tokens = meter.CreateCounter<long>("vietwash.ai.tokens", "{token}");
    }

    public void RecordRequest(string provider, bool success) =>
        requests.Add(1, Tags(("provider", provider), ("success", success)));

    public void RecordProviderDuration(string provider, double milliseconds, bool success) =>
        providerLatency.Record(
            milliseconds,
            Tags(("provider", provider), ("success", success))
        );

    public void RecordTool(string toolName, double milliseconds, bool success)
    {
        TagList tags = Tags(("tool_name", toolName), ("success", success));
        toolInvocations.Add(1, tags);
        toolLatency.Record(milliseconds, tags);
    }

    public void RecordToolRounds(int rounds) => toolRounds.Record(rounds);

    public void RecordTokenUsage(string provider, AiTokenUsage? usage)
    {
        if (usage?.InputTokens is int input)
            tokens.Add(input, Tags(("provider", provider), ("token_type", "input")));
        if (usage?.OutputTokens is int output)
            tokens.Add(output, Tags(("provider", provider), ("token_type", "output")));
        if (usage?.TotalTokens is int total)
            tokens.Add(total, Tags(("provider", provider), ("token_type", "total")));
    }

    public void Dispose() => meter.Dispose();

    private static TagList Tags(params (string Key, object Value)[] values)
    {
        TagList tags = default;
        foreach ((string key, object value) in values)
            tags.Add(key, value);
        return tags;
    }
}
