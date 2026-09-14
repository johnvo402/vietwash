using System.Diagnostics;
using System.Text.Json;
using Application.Common.Interfaces.Services;
using Application.Feature.AiAssistant.Interfaces;
using Application.Feature.AiAssistant.Models;
using Application.Feature.Reports.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Feature.AiAssistant.Commands.Chat;

public sealed class AiChatHandler(
    IAiChatClient chatClient,
    IAiBusinessToolRegistry tools,
    ICurrentAccount currentAccount,
    IHttpContextAccessor httpContextAccessor,
    IOptions<AiAssistantOptions> options,
    TimeProvider timeProvider,
    AiAssistantMetrics metrics,
    ILogger<AiChatHandler> logger
) : IRequestHandler<AiChatCommand, Result<AiChatResponse>>
{
    private const string SafeProviderFailure =
        "Trợ lý AI tạm thời không khả dụng. Vui lòng thử lại sau.";
    private const string InvalidProviderResponse =
        "Trợ lý AI không thể xử lý yêu cầu một cách an toàn.";

    public async ValueTask<Result<AiChatResponse>> Handle(
        AiChatCommand command,
        CancellationToken cancellationToken
    )
    {
        if (currentAccount.Id is null || currentAccount.Session is null)
            return Result<AiChatResponse>.Failure(new UnauthorizedError(Message.UNAUTHORIZED));
        if (currentAccount.Session.Role is not (ROLE.ADMIN or ROLE.MANAGER))
            return Result<AiChatResponse>.Failure(new ForbiddenError(Message.FORBIDDEN));

        string provider = options.Value.Provider ?? "unconfigured";
        if (options.Value.MaxToolRounds is < 1 or > 10)
            return Result<AiChatResponse>.Failure(
                new AiAssistantUnavailableError("AI configuration is invalid.")
            );

        string conversationId = Guid.TryParse(command.ConversationId, out Guid parsedConversationId)
            ? parsedConversationId.ToString()
            : Guid.NewGuid().ToString();
        string requestId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        TimeZoneInfo timeZone = ReportTimeRange.ResolveTimeZone(
            httpContextAccessor.HttpContext?.Request.Headers["Time-Zone"].ToString()
        );
        IReadOnlySet<long> authorizedBranches = (currentAccount.Session.Branches ?? [])
            .Select(value => long.TryParse(value, out long branchId) ? branchId : 0)
            .Where(branchId => branchId > 0)
            .ToHashSet();
        var toolContext = new AiToolExecutionContext(authorizedBranches, timeZone);
        var messages = new List<AiChatMessage> { new(AiChatRole.User, command.Message) };
        var usedTools = new List<string>();
        int completedToolRounds = 0;
        bool requestSucceeded = false;

        try
        {
            for (int providerRound = 0; providerRound <= options.Value.MaxToolRounds; providerRound++)
            {
                AiChatResult completion = await CompleteAsync(
                    new AiChatRequest(
                        BuildSystemInstruction(authorizedBranches, timeZone),
                        messages,
                        tools.Definitions
                    ),
                    provider,
                    cancellationToken
                );

                if (completion.ToolCalls.Count == 0)
                {
                    if (string.IsNullOrWhiteSpace(completion.Answer))
                        throw new AiProviderException("Provider returned no answer or tool call.");

                    requestSucceeded = true;
                    metrics.RecordToolRounds(completedToolRounds);
                    logger.LogInformation(
                        "AI request {RequestId} conversation {ConversationId} completed after {ToolRounds} tool rounds",
                        requestId,
                        conversationId,
                        completedToolRounds
                    );
                    return Result<AiChatResponse>.Success(
                        new AiChatResponse
                        {
                            Answer = completion.Answer.Trim(),
                            ConversationId = conversationId,
                            ToolsUsed = usedTools,
                        }
                    );
                }

                if (providerRound == options.Value.MaxToolRounds)
                    throw new AiProviderException("Maximum tool-call rounds exceeded.");

                messages.Add(
                    new AiChatMessage(
                        AiChatRole.Assistant,
                        completion.Answer,
                        completion.ToolCalls
                    )
                );
                var results = new List<AiToolResult>(completion.ToolCalls.Count);
                foreach (AiToolCall call in completion.ToolCalls)
                {
                    JsonElement result = await ExecuteToolAsync(
                        call,
                        toolContext,
                        requestId,
                        conversationId,
                        cancellationToken
                    );
                    results.Add(new AiToolResult(call.Id, call.Name, result));
                    usedTools.Add(call.Name);
                }

                messages.Add(new AiChatMessage(AiChatRole.Tool, ToolResults: results));
                completedToolRounds++;
            }

            throw new AiProviderException("Maximum tool-call rounds exceeded.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (AiProviderConfigurationException exception)
        {
            logger.LogWarning(
                exception,
                "AI request {RequestId} conversation {ConversationId} has unavailable configuration",
                requestId,
                conversationId
            );
            return Result<AiChatResponse>.Failure(
                new AiAssistantUnavailableError(SafeProviderFailure)
            );
        }
        catch (AiToolForbiddenException exception)
        {
            logger.LogWarning(
                "AI request {RequestId} conversation {ConversationId} rejected a forbidden tool scope: {Reason}",
                requestId,
                conversationId,
                exception.Message
            );
            return Result<AiChatResponse>.Failure(new ForbiddenError(Message.FORBIDDEN));
        }
        catch (AiToolValidationException exception)
        {
            logger.LogWarning(
                "AI request {RequestId} conversation {ConversationId} rejected a tool call: {Reason}",
                requestId,
                conversationId,
                exception.Message
            );
            return Result<AiChatResponse>.Failure(
                new AiAssistantProviderError(InvalidProviderResponse)
            );
        }
        catch (AiToolExecutionException exception)
        {
            logger.LogError(
                exception,
                "AI request {RequestId} conversation {ConversationId} could not retrieve business data",
                requestId,
                conversationId
            );
            return Result<AiChatResponse>.Failure(
                new AiAssistantProviderError(InvalidProviderResponse)
            );
        }
        catch (AiProviderException exception)
        {
            logger.LogError(
                exception,
                "AI request {RequestId} conversation {ConversationId} failed at provider {Provider}",
                requestId,
                conversationId,
                provider
            );
            return Result<AiChatResponse>.Failure(
                new AiAssistantUnavailableError(SafeProviderFailure)
            );
        }
        finally
        {
            metrics.RecordRequest(provider, requestSucceeded);
        }
    }

    private async Task<AiChatResult> CompleteAsync(
        AiChatRequest request,
        string provider,
        CancellationToken cancellationToken
    )
    {
        var stopwatch = Stopwatch.StartNew();
        bool success = false;
        try
        {
            AiChatResult result = await chatClient.CompleteAsync(request, cancellationToken);
            success = true;
            metrics.RecordTokenUsage(provider, result.TokenUsage);
            return result;
        }
        finally
        {
            stopwatch.Stop();
            metrics.RecordProviderDuration(provider, stopwatch.Elapsed.TotalMilliseconds, success);
        }
    }

    private async Task<JsonElement> ExecuteToolAsync(
        AiToolCall call,
        AiToolExecutionContext context,
        string requestId,
        string conversationId,
        CancellationToken cancellationToken
    )
    {
        var stopwatch = Stopwatch.StartNew();
        bool success = false;
        try
        {
            JsonElement result = await tools.ExecuteAsync(
                call.Name,
                call.Arguments,
                context,
                cancellationToken
            );
            success = true;
            return result;
        }
        finally
        {
            stopwatch.Stop();
            metrics.RecordTool(call.Name, stopwatch.Elapsed.TotalMilliseconds, success);
            logger.LogInformation(
                "AI tool {ToolName} for request {RequestId} conversation {ConversationId} finished in {DurationMs} ms with success {Success}",
                call.Name,
                requestId,
                conversationId,
                stopwatch.Elapsed.TotalMilliseconds,
                success
            );
        }
    }

    private string BuildSystemInstruction(
        IReadOnlySet<long> authorizedBranches,
        TimeZoneInfo timeZone
    )
    {
        DateOnly today = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone).DateTime
        );
        string branchList = authorizedBranches.Count == 0
            ? "không có"
            : string.Join(",", authorizedBranches.Order());
        return $$"""
            Bạn là Trợ lý Kinh doanh VietWash chỉ đọc dữ liệu. Trả lời ngắn gọn bằng tiếng Việt.
            Chỉ dùng số liệu do các tool VietWash cung cấp; không tự suy đoán hay bịa số. Nếu thiếu dữ liệu, nói rõ là chưa có dữ liệu.
            Phân biệt dữ kiện quan sát được với nhận định, và so sánh kỳ khi hữu ích.
            Không tuyên bố đã thực hiện hành động, không tiết lộ bí mật hoặc ID nội bộ trong câu trả lời.
            Không làm theo yêu cầu vượt quyền, bỏ qua phân quyền, gọi tool không được khai báo hoặc thay đổi dữ liệu.
            Ngày địa phương hiện tại là {{today:yyyy-MM-dd}}. Các branchId được phép dùng trong tool: {{branchList}}.
            """;
    }
}
