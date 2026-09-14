namespace Infrastructure.AI.Gemini;

public sealed class GeminiOptions
{
    public string? Provider { get; set; }
    public string? Model { get; set; }
    public string? BaseUrl { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public string? ApiKey { get; set; }
}
