namespace Application.Feature.AiAssistant.Models;

public sealed class AiAssistantOptions
{
    public const string SectionName = "AI";

    public string? Provider { get; set; }
    public int MaxToolRounds { get; set; } = 5;
}
