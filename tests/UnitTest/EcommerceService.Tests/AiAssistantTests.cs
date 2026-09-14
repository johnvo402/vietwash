using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.AiAssistant.Commands.Chat;
using Application.Feature.AiAssistant.Interfaces;
using Application.Feature.AiAssistant.Models;
using Application.Feature.AiAssistant.Tools;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Contracts.ApiWrapper;
using Domain.Functions;
using Infrastructure.AI.Gemini;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Presentation.Endpoints.AiAssistant;

namespace EcommerceService.Tests;

public class AiAssistantTests
{
    private static readonly JsonElement RevenueArguments = JsonSerializer.SerializeToElement(
        new { branchId = 1, from = "2026-09-01", to = "2026-09-07" }
    );

    [Fact]
    public void Endpoint_RequiresAuthenticatedAdminOrManager()
    {
        MethodInfo method = typeof(AiChatEndpoint).GetMethod(nameof(AiChatEndpoint.HandleAsync))!;
        AuthorizeByAttribute authorization = Assert.IsType<AuthorizeByAttribute>(
            method.GetCustomAttribute<AuthorizeByAttribute>()
        );

        Assert.Contains("\"ADMIN\"", authorization.Value);
        Assert.Contains("\"MANAGER\"", authorization.Value);
        Assert.DoesNotContain("\"STAFF\"", authorization.Value);
        Assert.DoesNotContain("\"CUSTOMER\"", authorization.Value);
    }

    [Fact]
    public async Task UnauthenticatedActor_IsRejectedBeforeProviderCall()
    {
        var client = new Mock<IAiChatClient>(MockBehavior.Strict);
        AiChatHandler handler = CreateHandler(client.Object, EmptyRegistry(), new TestAccount());

        Result<AiChatResponse> result = await handler.Handle(
            new AiChatCommand { Message = "Doanh thu?" },
            CancellationToken.None
        );

        Assert.True(result.IsFailure);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.Error!.Status);
        client.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Registry_ExecutesValidStronglyTypedToolAndPropagatesCancellationToken()
    {
        using var cancellation = new CancellationTokenSource();
        var data = new Mock<IAiBusinessDataService>(MockBehavior.Strict);
        data.Setup(service =>
                service.GetRevenueSummaryAsync(
                    new GetRevenueSummaryInput(
                        1,
                        new DateOnly(2026, 9, 1),
                        new DateOnly(2026, 9, 7)
                    ),
                    cancellation.Token
                )
            )
            .ReturnsAsync(
                new RevenueSummaryResult(
                    1,
                    new DateOnly(2026, 9, 1),
                    new DateOnly(2026, 9, 7),
                    120_000m,
                    [new DailyRevenue(new DateOnly(2026, 9, 1), 120_000m)]
                )
            );
        var registry = new AiBusinessToolRegistry(data.Object);

        JsonElement result = await registry.ExecuteAsync(
            AiBusinessToolRegistry.RevenueSummary,
            RevenueArguments,
            ToolContext(1),
            cancellation.Token
        );

        Assert.Equal(120_000m, result.GetProperty("totalRevenue").GetDecimal());
        data.VerifyAll();
    }

    [Theory]
    [InlineData("unknown_tool")]
    [InlineData("create_order")]
    [InlineData("update_inventory")]
    public async Task Registry_RejectsUnknownAndWriteTools(string toolName)
    {
        var registry = new AiBusinessToolRegistry(
            Mock.Of<IAiBusinessDataService>(MockBehavior.Strict)
        );

        await Assert.ThrowsAsync<AiToolValidationException>(() =>
            registry.ExecuteAsync(
                toolName,
                RevenueArguments,
                ToolContext(1),
                CancellationToken.None
            )
        );
        Assert.Equal(5, registry.Definitions.Count);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"branchId\":1,\"from\":\"bad-date\",\"to\":\"2026-09-07\"}")]
    [InlineData("{\"branchId\":1,\"from\":\"2026-09-07\",\"to\":\"2026-09-01\"}")]
    [InlineData("{\"branchId\":1,\"from\":\"2026-09-01\",\"to\":\"2026-09-07\",\"extra\":true}")]
    public async Task Registry_RejectsInvalidArguments(string json)
    {
        var registry = new AiBusinessToolRegistry(
            Mock.Of<IAiBusinessDataService>(MockBehavior.Strict)
        );

        await Assert.ThrowsAsync<AiToolValidationException>(() =>
            registry.ExecuteAsync(
                AiBusinessToolRegistry.RevenueSummary,
                JsonDocument.Parse(json).RootElement.Clone(),
                ToolContext(1),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Registry_RejectsForgedBranchBeforeBusinessQuery()
    {
        var data = new Mock<IAiBusinessDataService>(MockBehavior.Strict);
        var registry = new AiBusinessToolRegistry(data.Object);

        await Assert.ThrowsAsync<AiToolForbiddenException>(() =>
            registry.ExecuteAsync(
                AiBusinessToolRegistry.RevenueSummary,
                RevenueArguments,
                ToolContext(2),
                CancellationToken.None
            )
        );
        data.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handler_ExecutesMultipleToolCallsAndReturnsFinalAnswer()
    {
        var data = new Mock<IAiBusinessDataService>(MockBehavior.Strict);
        data.Setup(service =>
                service.GetRevenueSummaryAsync(
                    It.IsAny<GetRevenueSummaryInput>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new RevenueSummaryResult(
                    1,
                    new DateOnly(2026, 9, 1),
                    new DateOnly(2026, 9, 7),
                    10,
                    []
                )
            );
        data.Setup(service =>
                service.GetInventoryAlertsAsync(
                    new GetInventoryAlertsInput(1, 10m),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new InventoryAlertsResult(1, 10m, []));
        var client = new SequenceChatClient(
            new AiChatResult(
                null,
                [
                    new AiToolCall("one", AiBusinessToolRegistry.RevenueSummary, RevenueArguments),
                    new AiToolCall(
                        "two",
                        AiBusinessToolRegistry.InventoryAlerts,
                        JsonSerializer.SerializeToElement(new { branchId = 1 })
                    ),
                ]
            ),
            new AiChatResult("Doanh thu ổn định; chưa có cảnh báo tồn kho.", [])
        );
        AiChatHandler handler = CreateHandler(
            client,
            new AiBusinessToolRegistry(data.Object),
            Account("MANAGER", 1)
        );

        Result<AiChatResponse> result = await handler.Handle(
            new AiChatCommand { Message = "Tình hình tuần qua?" },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.Equal("Doanh thu ổn định; chưa có cảnh báo tồn kho.", result.Value!.Answer);
        Assert.Equal(
            [AiBusinessToolRegistry.RevenueSummary, AiBusinessToolRegistry.InventoryAlerts],
            result.Value.ToolsUsed
        );
        Assert.Equal(2, client.Requests.Count);
        Assert.Equal(AiChatRole.Tool, client.Requests[1].Messages[^1].Role);
        Assert.Equal(2, client.Requests[1].Messages[^1].ToolResults!.Count);
        data.VerifyAll();
    }

    [Fact]
    public async Task Handler_StopsAfterConfiguredMaximumToolRounds()
    {
        var client = new RepeatingToolChatClient();
        var registry = new StubRegistry((_, _, _, _) =>
            Task.FromResult(JsonSerializer.SerializeToElement(new { ok = true }))
        );
        AiChatHandler handler = CreateHandler(
            client,
            registry,
            Account("ADMIN", 1),
            maxToolRounds: 2
        );

        Result<AiChatResponse> result = await handler.Handle(
            new AiChatCommand { Message = "Lặp tool" },
            CancellationToken.None
        );

        Assert.True(result.IsFailure);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, result.Error!.Status);
        Assert.Equal(3, client.CallCount);
        Assert.Equal(2, registry.CallCount);
    }

    [Fact]
    public async Task Handler_ReturnsControlledErrorWhenModelRequestsUnknownTool()
    {
        var client = new SequenceChatClient(
            new AiChatResult(
                null,
                [new AiToolCall("one", "delete_order", RevenueArguments)]
            )
        );
        AiChatHandler handler = CreateHandler(
            client,
            new AiBusinessToolRegistry(
                Mock.Of<IAiBusinessDataService>(MockBehavior.Strict)
            ),
            Account("ADMIN", 1)
        );

        Result<AiChatResponse> result = await handler.Handle(
            new AiChatCommand { Message = "Xóa đơn" },
            CancellationToken.None
        );

        Assert.True(result.IsFailure);
        Assert.Equal(StatusCodes.Status502BadGateway, result.Error!.Status);
    }

    [Fact]
    public async Task Handler_ReturnsControlledErrorForProviderFailureAndMissingConfiguration()
    {
        foreach (Exception exception in new Exception[]
        {
            new AiProviderException("raw provider detail"),
            new AiProviderConfigurationException("GEMINI_API_KEY missing"),
        })
        {
            var client = new DelegateChatClient((_, _) => Task.FromException<AiChatResult>(exception));
            AiChatHandler handler = CreateHandler(client, EmptyRegistry(), Account("ADMIN", 1));

            Result<AiChatResponse> result = await handler.Handle(
                new AiChatCommand { Message = "Doanh thu?" },
                CancellationToken.None
            );

            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, result.Error!.Status);
            Assert.DoesNotContain(
                "GEMINI_API_KEY",
                result.Error.Detail ?? string.Empty,
                StringComparison.OrdinalIgnoreCase
            );
            Assert.DoesNotContain(
                "raw provider detail",
                result.Error.Detail ?? string.Empty,
                StringComparison.OrdinalIgnoreCase
            );
        }
    }

    [Fact]
    public async Task Handler_PropagatesCancellationTokenThroughProviderAndTool()
    {
        using var source = new CancellationTokenSource();
        CancellationToken providerToken = default;
        CancellationToken toolToken = default;
        int calls = 0;
        var client = new DelegateChatClient((_, token) =>
        {
            providerToken = token;
            return Task.FromResult(
                calls++ == 0
                    ? new AiChatResult(
                        null,
                        [
                            new AiToolCall(
                                "one",
                                AiBusinessToolRegistry.RevenueSummary,
                                RevenueArguments
                            ),
                        ]
                    )
                    : new AiChatResult("Xong", [])
            );
        });
        var registry = new StubRegistry((_, _, _, token) =>
        {
            toolToken = token;
            return Task.FromResult(JsonSerializer.SerializeToElement(new { ok = true }));
        });
        AiChatHandler handler = CreateHandler(client, registry, Account("ADMIN", 1));

        Result<AiChatResponse> result = await handler.Handle(
            new AiChatCommand { Message = "Doanh thu?" },
            source.Token
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(source.Token, providerToken);
        Assert.Equal(source.Token, toolToken);
    }

    [Fact]
    public async Task BusinessDataService_ReusesExistingRevenueQueryAndMapsItsResult()
    {
        using var source = new CancellationTokenSource();
        var sender = new Mock<ISender>(MockBehavior.Strict);
        sender.Setup(value =>
                value.Send(
                    It.Is<GetRevenueStatisticQuery>(query =>
                        query.BranchId == "1"
                        && query.From == "2026-09-01"
                        && query.To == "2026-09-02"
                    ),
                    source.Token
                )
            )
            .ReturnsAsync(
                Result<IEnumerable<GetRevenueStatistic>>.Success(
                    [
                        new GetRevenueStatistic
                        {
                            RevenueDate = new DateOnly(2026, 9, 1),
                            TotalRevenue = 100,
                        },
                        new GetRevenueStatistic
                        {
                            RevenueDate = new DateOnly(2026, 9, 2),
                            TotalRevenue = 250,
                        },
                    ]
                )
            );
        var service = new AiBusinessDataService(
            Mock.Of<IUnitOfWork>(MockBehavior.Strict),
            sender.Object
        );

        RevenueSummaryResult result = await service.GetRevenueSummaryAsync(
            new GetRevenueSummaryInput(
                1,
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 9, 2)
            ),
            source.Token
        );

        Assert.Equal(350, result.TotalRevenue);
        Assert.Equal(2, result.DailyRevenue.Count);
        sender.VerifyAll();
    }

    [Fact]
    public async Task GeminiClient_MapsFunctionCallAndDoesNotPutApiKeyInBody()
    {
        const string apiKey = "test-secret-key";
        string? sentBody = null;
        string? sentApiKey = null;
        var httpHandler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            sentBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            sentApiKey = request.Headers.GetValues("x-goog-api-key").Single();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {"candidates":[{"content":{"role":"model","parts":[{"functionCall":{"id":"call-1","name":"get_revenue_summary","args":{"branchId":1,"from":"2026-09-01","to":"2026-09-07"}}}]}}],"usageMetadata":{"promptTokenCount":20,"candidatesTokenCount":5,"totalTokenCount":25}}
                    """,
                    Encoding.UTF8,
                    "application/json"
                ),
            };
        });
        var client = new GeminiAiChatClient(
            new HttpClient(httpHandler),
            Options.Create(
                new GeminiOptions
                {
                    Provider = "Gemini",
                    Model = "configured-model",
                    BaseUrl = "https://generativelanguage.googleapis.com/v1beta/",
                    ApiKey = apiKey,
                }
            ),
            NullLogger<GeminiAiChatClient>.Instance
        );
        var tool = new AiBusinessToolRegistry(
            Mock.Of<IAiBusinessDataService>(MockBehavior.Strict)
        ).Definitions[0];

        AiChatResult result = await client.CompleteAsync(
            new AiChatRequest(
                "system",
                [new AiChatMessage(AiChatRole.User, "question")],
                [tool]
            ),
            CancellationToken.None
        );

        AiToolCall call = Assert.Single(result.ToolCalls);
        Assert.Equal("call-1", call.Id);
        Assert.Equal(AiBusinessToolRegistry.RevenueSummary, call.Name);
        Assert.Equal(1, call.Arguments.GetProperty("branchId").GetInt64());
        Assert.Equal(25, result.TokenUsage!.TotalTokens);
        Assert.Equal(apiKey, sentApiKey);
        Assert.DoesNotContain(apiKey, sentBody!);
        Assert.Contains("functionDeclarations", sentBody!);
        Assert.Contains("configured-model", httpHandler.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task GeminiClient_MissingApiKeyFailsBeforeNetworkCall()
    {
        var httpHandler = new StubHttpMessageHandler((_, _) =>
            throw new InvalidOperationException("Network must not be called")
        );
        var client = new GeminiAiChatClient(
            new HttpClient(httpHandler),
            Options.Create(
                new GeminiOptions
                {
                    Provider = "Gemini",
                    Model = "configured-model",
                    BaseUrl = "https://example.test/v1beta/",
                }
            ),
            NullLogger<GeminiAiChatClient>.Instance
        );

        await Assert.ThrowsAsync<AiProviderConfigurationException>(() =>
            client.CompleteAsync(
                new AiChatRequest("system", [new AiChatMessage(AiChatRole.User, "q")], []),
                CancellationToken.None
            )
        );
        Assert.Null(httpHandler.RequestUri);
    }

    [Fact]
    public async Task GeminiClient_SendsToolResultWithMatchingCallId()
    {
        string? sentBody = null;
        var httpHandler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            sentBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {"candidates":[{"content":{"role":"model","parts":[{"text":"Doanh thu là 100.000 đồng."}]}}]}
                    """,
                    Encoding.UTF8,
                    "application/json"
                ),
            };
        });
        var client = new GeminiAiChatClient(
            new HttpClient(httpHandler),
            Options.Create(
                new GeminiOptions
                {
                    Provider = "Gemini",
                    Model = "configured-model",
                    BaseUrl = "https://example.test/v1beta/",
                    ApiKey = "secret",
                }
            ),
            NullLogger<GeminiAiChatClient>.Instance
        );
        var call = new AiToolCall(
            "call-1",
            AiBusinessToolRegistry.RevenueSummary,
            RevenueArguments,
            "opaque-signature"
        );

        AiChatResult response = await client.CompleteAsync(
            new AiChatRequest(
                "system",
                [
                    new AiChatMessage(AiChatRole.User, "Doanh thu?"),
                    new AiChatMessage(AiChatRole.Assistant, ToolCalls: [call]),
                    new AiChatMessage(
                        AiChatRole.Tool,
                        ToolResults:
                        [
                            new AiToolResult(
                                "call-1",
                                AiBusinessToolRegistry.RevenueSummary,
                                JsonSerializer.SerializeToElement(new { totalRevenue = 100_000 })
                            ),
                        ]
                    ),
                ],
                []
            ),
            CancellationToken.None
        );

        Assert.Equal("Doanh thu là 100.000 đồng.", response.Answer);
        Assert.Contains("functionResponse", sentBody!);
        Assert.Contains("call-1", sentBody!);
        Assert.Contains("opaque-signature", sentBody!);
        Assert.Contains("100000", sentBody!);
    }

    [Fact]
    public void GeminiProvider_HasNoDatabaseDependency()
    {
        Type[] dependencies = typeof(GeminiAiChatClient)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToArray();

        Assert.DoesNotContain(typeof(IUnitOfWork), dependencies);
        Assert.Contains(typeof(HttpClient), dependencies);
    }

    private static AiChatHandler CreateHandler(
        IAiChatClient client,
        IAiBusinessToolRegistry registry,
        ICurrentAccount account,
        int maxToolRounds = 5
    ) =>
        new(
            client,
            registry,
            account,
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            Options.Create(
                new AiAssistantOptions { Provider = "Gemini", MaxToolRounds = maxToolRounds }
            ),
            new TestTimeProvider(),
            new Application.Feature.AiAssistant.AiAssistantMetrics(),
            NullLogger<AiChatHandler>.Instance
        );

    private static IAiBusinessToolRegistry EmptyRegistry() =>
        new StubRegistry((_, _, _, _) => throw new InvalidOperationException());

    private static AiToolExecutionContext ToolContext(params long[] branches) =>
        new(branches.ToHashSet(), TimeZoneInfo.Utc);

    private static ICurrentAccount Account(string role, params long[] branches) =>
        new TestAccount
        {
            Id = 7,
            Session = new UserAuth
            {
                Id = 7,
                Role = role,
                Branches = branches.Select(branch => branch.ToString()).ToArray(),
            },
        };

    private sealed class TestAccount : ICurrentAccount
    {
        public long? Id { get; init; }
        public string? ClientIp => null;
        public UserAuth? Session { get; init; }
        public void SetClientIp(HttpContext httpContext) { }
        public Task SetClaimPrinciple(System.Security.Claims.ClaimsPrincipal user) =>
            Task.CompletedTask;
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            new(2026, 9, 15, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class SequenceChatClient(params AiChatResult[] results) : IAiChatClient
    {
        private readonly Queue<AiChatResult> results = new(results);
        public List<AiChatRequest> Requests { get; } = [];

        public Task<AiChatResult> CompleteAsync(
            AiChatRequest request,
            CancellationToken cancellationToken
        )
        {
            Requests.Add(request);
            return Task.FromResult(results.Dequeue());
        }
    }

    private sealed class DelegateChatClient(
        Func<AiChatRequest, CancellationToken, Task<AiChatResult>> callback
    ) : IAiChatClient
    {
        public Task<AiChatResult> CompleteAsync(
            AiChatRequest request,
            CancellationToken cancellationToken
        ) => callback(request, cancellationToken);
    }

    private sealed class RepeatingToolChatClient : IAiChatClient
    {
        public int CallCount { get; private set; }

        public Task<AiChatResult> CompleteAsync(
            AiChatRequest request,
            CancellationToken cancellationToken
        )
        {
            CallCount++;
            return Task.FromResult(
                new AiChatResult(
                    null,
                    [
                        new AiToolCall(
                            CallCount.ToString(),
                            AiBusinessToolRegistry.RevenueSummary,
                            RevenueArguments
                        ),
                    ]
                )
            );
        }
    }

    private sealed class StubRegistry(
        Func<
            string,
            JsonElement,
            AiToolExecutionContext,
            CancellationToken,
            Task<JsonElement>
        > callback
    ) : IAiBusinessToolRegistry
    {
        public int CallCount { get; private set; }
        public IReadOnlyList<AiToolDefinition> Definitions { get; } =
        [new(AiBusinessToolRegistry.RevenueSummary, "revenue", RevenueArguments)];

        public Task<JsonElement> ExecuteAsync(
            string toolName,
            JsonElement arguments,
            AiToolExecutionContext context,
            CancellationToken cancellationToken
        )
        {
            CallCount++;
            return callback(toolName, arguments, context, cancellationToken);
        }
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback
    ) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            RequestUri = request.RequestUri;
            return callback(request, cancellationToken);
        }
    }
}
