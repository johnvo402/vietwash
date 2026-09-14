using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Feature.AiAssistant.Interfaces;
using Application.Feature.AiAssistant.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.AI.Gemini;

public sealed class GeminiAiChatClient(
    HttpClient httpClient,
    IOptions<GeminiOptions> options,
    ILogger<GeminiAiChatClient> logger
) : IAiChatClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<AiChatResult> CompleteAsync(
        AiChatRequest request,
        CancellationToken cancellationToken
    )
    {
        GeminiOptions configuration = options.Value;
        Validate(configuration);
        Uri endpoint = BuildEndpoint(configuration);
        GeminiGenerateContentRequest payload = Map(request);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(payload, options: JsonOptions),
        };
        httpRequest.Headers.TryAddWithoutValidation("x-goog-api-key", configuration.ApiKey);

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Gemini request failed with HTTP status {StatusCode}",
                    (int)response.StatusCode
                );
                throw new AiProviderException("Gemini request was unsuccessful.");
            }

            GeminiGenerateContentResponse? content = await response.Content.ReadFromJsonAsync<
                GeminiGenerateContentResponse
            >(JsonOptions, cancellationToken);
            return Map(content);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException exception)
        {
            throw new AiProviderException("Gemini request timed out.", exception);
        }
        catch (HttpRequestException exception)
        {
            throw new AiProviderException("Gemini network request failed.", exception);
        }
        catch (JsonException exception)
        {
            throw new AiProviderException("Gemini returned malformed JSON.", exception);
        }
    }

    private static void Validate(GeminiOptions configuration)
    {
        if (!string.Equals(configuration.Provider, "Gemini", StringComparison.OrdinalIgnoreCase))
            throw new AiProviderConfigurationException("AI provider is not configured for Gemini.");
        if (string.IsNullOrWhiteSpace(configuration.ApiKey))
            throw new AiProviderConfigurationException("GEMINI_API_KEY is missing.");
        if (string.IsNullOrWhiteSpace(configuration.Model))
            throw new AiProviderConfigurationException("Gemini model is missing.");
        if (
            string.IsNullOrWhiteSpace(configuration.BaseUrl)
            || !Uri.TryCreate(configuration.BaseUrl, UriKind.Absolute, out _)
        )
            throw new AiProviderConfigurationException("Gemini base URL is invalid.");
    }

    private static Uri BuildEndpoint(GeminiOptions configuration)
    {
        var baseUri = new Uri(configuration.BaseUrl!.TrimEnd('/') + "/", UriKind.Absolute);
        return new Uri(
            baseUri,
            $"models/{Uri.EscapeDataString(configuration.Model!)}:generateContent"
        );
    }

    private static GeminiGenerateContentRequest Map(AiChatRequest request) =>
        new(
            new GeminiContent(null, [new GeminiPart(Text: request.SystemInstruction)]),
            request.Messages.Select(Map).ToList(),
            [
                new GeminiTool(
                    request.Tools.Select(tool =>
                            new GeminiFunctionDeclaration(
                                tool.Name,
                                tool.Description,
                                tool.InputSchema
                            )
                        )
                        .ToList()
                ),
            ],
            new GeminiToolConfig(new GeminiFunctionCallingConfig("VALIDATED"))
        );

    private static GeminiContent Map(AiChatMessage message)
    {
        string role = message.Role == AiChatRole.Assistant ? "model" : "user";
        var parts = new List<GeminiPart>();
        if (!string.IsNullOrWhiteSpace(message.Text))
            parts.Add(new GeminiPart(Text: message.Text));
        if (message.ToolCalls is not null)
            parts.AddRange(
                message.ToolCalls.Select(call =>
                    new GeminiPart(
                        FunctionCall: new GeminiFunctionCall(
                            call.Id,
                            call.Name,
                            call.Arguments
                        ),
                        ThoughtSignature: call.ProviderMetadata
                    )
                )
            );
        if (message.ToolResults is not null)
            parts.AddRange(
                message.ToolResults.Select(result =>
                    new GeminiPart(
                        FunctionResponse: new GeminiFunctionResponse(
                            result.CallId,
                            result.Name,
                            JsonSerializer.SerializeToElement(
                                new { result = result.Result },
                                JsonOptions
                            )
                        )
                    )
                )
            );
        return new GeminiContent(role, parts);
    }

    private static AiChatResult Map(GeminiGenerateContentResponse? response)
    {
        GeminiCandidate candidate = response?.Candidates?.FirstOrDefault()
            ?? throw new AiProviderException("Gemini returned no candidate.");
        IReadOnlyList<GeminiPart> parts = candidate.Content?.Parts ?? [];
        List<AiToolCall> calls = parts
            .Where(part => part.FunctionCall is not null)
            .Select(part =>
            {
                GeminiFunctionCall call = part.FunctionCall!;
                JsonElement arguments = call.Args.ValueKind == JsonValueKind.Undefined
                    ? JsonSerializer.SerializeToElement(new { }, JsonOptions)
                    : call.Args.Clone();
                return new AiToolCall(
                    call.Id,
                    call.Name ?? string.Empty,
                    arguments,
                    part.ThoughtSignature
                );
            })
            .ToList();
        string? answer = string.Join(
            Environment.NewLine,
            parts
                .Select(part => part.Text)
                .Where(text => !string.IsNullOrWhiteSpace(text))
        );
        if (string.IsNullOrWhiteSpace(answer))
            answer = null;

        GeminiUsageMetadata? usage = response?.UsageMetadata;
        return new AiChatResult(
            answer,
            calls,
            usage is null
                ? null
                : new AiTokenUsage(
                    usage.PromptTokenCount,
                    usage.CandidatesTokenCount,
                    usage.TotalTokenCount
                )
        );
    }

    private sealed record GeminiGenerateContentRequest(
        GeminiContent SystemInstruction,
        IReadOnlyList<GeminiContent> Contents,
        IReadOnlyList<GeminiTool> Tools,
        GeminiToolConfig ToolConfig
    );

    private sealed record GeminiContent(string? Role, IReadOnlyList<GeminiPart> Parts);

    private sealed record GeminiPart(
        string? Text = null,
        GeminiFunctionCall? FunctionCall = null,
        GeminiFunctionResponse? FunctionResponse = null,
        string? ThoughtSignature = null
    );

    private sealed record GeminiTool(
        IReadOnlyList<GeminiFunctionDeclaration> FunctionDeclarations
    );

    private sealed record GeminiFunctionDeclaration(
        string Name,
        string Description,
        JsonElement Parameters
    );

    private sealed record GeminiToolConfig(GeminiFunctionCallingConfig FunctionCallingConfig);

    private sealed record GeminiFunctionCallingConfig(string Mode);

    private sealed record GeminiFunctionCall(string? Id, string? Name, JsonElement Args);

    private sealed record GeminiFunctionResponse(string? Id, string Name, JsonElement Response);

    private sealed record GeminiGenerateContentResponse(
        IReadOnlyList<GeminiCandidate>? Candidates,
        GeminiUsageMetadata? UsageMetadata
    );

    private sealed record GeminiCandidate(GeminiContent? Content, string? FinishReason);

    private sealed record GeminiUsageMetadata(
        int? PromptTokenCount,
        int? CandidatesTokenCount,
        int? TotalTokenCount
    );
}
