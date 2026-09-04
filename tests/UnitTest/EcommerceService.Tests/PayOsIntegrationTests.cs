using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Command.UpdateStatus;
using Application.Feature.Orders.Queries.GetLinkPayment;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Infrastructure.Services.PayOs;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Net.payOS.Errors;
using Net.payOS.Types;
using Presentation.Endpoints.Orders;
using Presentation.Endpoints.Webhooks;
using Presentation.Routes;
using Shared.Kernel.Common.Specs.Interfaces;

namespace EcommerceService.Tests;

public class PayOsSettingTests
{
    [Theory]
    [InlineData("http://app.example.com/payment")]
    [InlineData("https://localhost/payment")]
    [InlineData("https://127.0.0.1/payment")]
    [InlineData("https://10.0.0.2/payment")]
    [InlineData("https://app.local/payment")]
    [InlineData("https://user:pass@app.example.com/payment")]
    public void StagingAndProductionRejectNonPublicHttpsUrls(string url)
    {
        var setting = ValidSetting();
        setting.ReturnUrl = url;
        Assert.Contains(PayOsSettingValidator.GetErrors(setting, requirePublicHttps: true), x => x.Contains("ReturnUrl"));
    }

    [Fact]
    public void StagingWebhookMustMatchThePublishedEdgeRoute()
    {
        var setting = ValidSetting();
        Assert.Empty(PayOsSettingValidator.GetErrors(setting, true));
        setting.WebhookUrl = "https://api.example.com/wrong-route";
        Assert.Contains(PayOsSettingValidator.GetErrors(setting, true), x => x.Contains("/Webhook/api/CompletedOrder"));
    }

    [Fact]
    public void DisabledConfiguration_AllowsApplicationStartupWithoutCredentials()
    {
        Assert.Empty(PayOsSettingValidator.GetErrors(new PayOsSetting { IsEnabled = false }));
    }

    [Fact]
    public void DisabledConfiguration_RegistersSafeClientAndVerifierFallbacks()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["PayOsSetting:IsEnabled"] = "false" }
            )
            .Build();
        ServiceProvider provider = new ServiceCollection()
            .AddPayOs(configuration)
            .BuildServiceProvider();

        Assert.IsType<UnavailableOrderPaymentLinkClient>(
            provider.GetRequiredService<IOrderPaymentLinkClient>()
        );
        Assert.IsType<UnavailablePayOsWebhookVerifier>(
            provider.GetRequiredService<IOrderPaymentWebhookVerifier>()
        );
    }

    [Fact]
    public void HalfConfiguredEnabledProvider_FailsStartupWithClearValidation()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["PayOsSetting:IsEnabled"] = "true",
                    ["PayOsSetting:ClientId"] = "client",
                }
            )
            .Build();

        OptionsValidationException error = Assert.Throws<OptionsValidationException>(
            () => new ServiceCollection().AddPayOs(configuration)
        );

        Assert.Contains("ApiKey", error.Message);
        Assert.Contains("WebhookUrl", error.Message);
    }

    [Theory]
    [InlineData("ClientId")]
    [InlineData("ApiKey")]
    [InlineData("ChecksumKey")]
    [InlineData("ReturnUrl")]
    [InlineData("CancelUrl")]
    [InlineData("WebhookUrl")]
    public void EnabledConfiguration_RequiresEverySetting(string missingSetting)
    {
        PayOsSetting setting = ValidSetting();
        typeof(PayOsSetting).GetProperty(missingSetting)!.SetValue(setting, null);

        string error = Assert.Single(PayOsSettingValidator.GetErrors(setting));

        Assert.Contains(missingSetting, error);
    }

    [Theory]
    [InlineData("return.test/path")]
    [InlineData("ftp://return.test/path")]
    [InlineData("not a url")]
    public void EnabledConfiguration_RejectsNonHttpAbsoluteUrls(string returnUrl)
    {
        PayOsSetting setting = ValidSetting();
        setting.ReturnUrl = returnUrl;

        string error = Assert.Single(PayOsSettingValidator.GetErrors(setting));

        Assert.Contains(nameof(PayOsSetting.ReturnUrl), error);
    }

    [Theory]
    [InlineData("http://localhost:3000/payment/payos-return")]
    [InlineData("https://app.vietwash.vn/payment/payos-return")]
    public void EnabledConfiguration_AcceptsAbsoluteHttpUrls(string returnUrl)
    {
        PayOsSetting setting = ValidSetting();
        setting.ReturnUrl = returnUrl;

        Assert.Empty(PayOsSettingValidator.GetErrors(setting));
    }

    private static PayOsSetting ValidSetting() =>
        new()
        {
            IsEnabled = true,
            ClientId = "client",
            ApiKey = "api",
            ChecksumKey = "checksum",
            ReturnUrl = "https://app.test/payment/payos-return",
            CancelUrl = "https://app.test/payment/payos-return",
            WebhookUrl = "https://api.test/Webhook/api/CompletedOrder",
        };
}

public class PayOsAuthorityHandlerTests
{
    [Fact]
    public async Task VerifiedWebhookForUnknownOrder_ReturnsNotFound()
    {
        (UpdateStatusHandler handler, _) = CreateHandler(null);

        Result result = await handler.Handle(
            UpdateStatusCommand.FromVerifiedPayOsWebhook(
                10,
                100,
                new OrderUpdateStatus
                {
                    Status = OrderStatus.Completed,
                    PaymentMethod = PaymentMethod.Card,
                }
            ),
            default
        );

        Assert.Equal(404, result.Error?.Status);
    }

    [Fact]
    public async Task VerifiedWebhookWithWrongAmount_ReturnsBadRequestWithoutSideEffects()
    {
        Order order = new(1, 99, "ORD-1", 100, 100, OrderStatus.Processed);
        (UpdateStatusHandler handler, Mock<IUnitOfWork> unitOfWork) = CreateHandler(order);

        Result result = await handler.Handle(
            UpdateStatusCommand.FromVerifiedPayOsWebhook(
                10,
                99,
                new OrderUpdateStatus
                {
                    Status = OrderStatus.Completed,
                    PaymentMethod = PaymentMethod.Card,
                }
            ),
            default
        );

        Assert.Equal(400, result.Error?.Status);
        Assert.Equal(OrderStatus.Processed, order.Status);
        Assert.Empty(order.UncommittedEvents);
        unitOfWork.Verify(work => work.Repository<Order>(It.IsAny<bool>()), Times.Never);
    }

    private static (UpdateStatusHandler, Mock<IUnitOfWork>) CreateHandler(Order? order)
    {
        Mock<DbTransaction> transaction = new();
        Mock<IDynamicSpecificationRepository<Order>> orders = new(MockBehavior.Strict);
        orders
            .Setup(repository =>
                repository.FindByConditionAsync(
                    It.IsAny<ISpecification<Order>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(order);
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        unitOfWork
            .Setup(work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);
        unitOfWork
            .Setup(work => work.DynamicReadOnlyRepository<Order>(false))
            .Returns(orders.Object);
        unitOfWork
            .Setup(work => work.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return (new UpdateStatusHandler(unitOfWork.Object, Mock.Of<ICurrentAccount>()), unitOfWork);
    }
}

public class PayOsOrderPolicyTests
{
    [Theory]
    [InlineData("PENDING", OrderPaymentLinkState.Pending)]
    [InlineData("processing", OrderPaymentLinkState.Processing)]
    [InlineData(" PAID ", OrderPaymentLinkState.Paid)]
    [InlineData("CANCELLED", OrderPaymentLinkState.Cancelled)]
    [InlineData("expired", OrderPaymentLinkState.Unknown)]
    [InlineData(null, OrderPaymentLinkState.Unknown)]
    public void ProviderStates_AreHandledExplicitly(
        string? providerState,
        OrderPaymentLinkState expected
    ) => Assert.Equal(expected, PayOsOrderPolicy.GetState(providerState));

    [Fact]
    public void Description_IsDeterministicAsciiAndProviderCompatible()
    {
        string description = PayOsOrderPolicy.GetDescription(
            "order-abcdefghijklmnopqrstuvwxyz-123",
            42
        );

        Assert.Equal("VW ORDERABCDEFGHIJKLMNOPQ", description);
        Assert.True(description.Length <= 25);
    }

    [Fact]
    public void Description_FallsBackToOrderIdWhenCodeHasNoSafeCharacters()
    {
        Assert.Equal("VW 42", PayOsOrderPolicy.GetDescription("---", 42));
    }
}

public class PayOsPaymentLinkHandlerTests
{
    [Fact]
    public async Task DisabledProvider_ReturnsBusinessErrorWithoutProviderCall()
    {
        PaymentHarness harness = CreateHarness(paymentSettings: DisabledSettings());

        Result<CreatePaymentResult> result = await harness.Handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10 },
            default
        );

        Assert.Equal(400, result.Error?.Status);
        Assert.Contains("unavailable", result.Error?.Title, StringComparison.OrdinalIgnoreCase);
        harness.Client.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.InProgress)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task OnlyProcessedOrders_AreEligibleAndNeverCallProvider(OrderStatus status)
    {
        PaymentHarness harness = CreateHarness(status);

        Result<CreatePaymentResult> result = await harness.Handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10 },
            default
        );

        Assert.Equal(400, result.Error?.Status);
        harness.Client.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExactNotFound_CreatesUsingServerUrlsAndAuthoritativeAmount()
    {
        PaymentHarness harness = CreateHarness();
        PaymentData? captured = null;
        harness
            .Client.Setup(client => client.GetPaymentLinkInformationAsync(10))
            .ThrowsAsync(new PayOSError(PayOsOrderPolicy.PaymentLinkNotFoundCode, "Not found"));
        harness
            .Client.Setup(client => client.CreatePaymentLinkAsync(It.IsAny<PaymentData>()))
            .Callback<PaymentData>(request => captured = request)
            .ReturnsAsync(Created("PENDING"));

        Result<CreatePaymentResult> result = await harness.Handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10 },
            default
        );

        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        Assert.Equal(10, captured.orderCode);
        Assert.Equal(100, captured.amount);
        Assert.Equal("VW ORD1", captured.description);
        Assert.Equal(EnabledSettings().ReturnUrl, captured.returnUrl);
        Assert.Equal(EnabledSettings().CancelUrl, captured.cancelUrl);
        Assert.Single(captured.items);
    }

    [Theory]
    [InlineData("PENDING")]
    [InlineData("PROCESSING")]
    [InlineData("PAID")]
    public async Task ExistingActiveOrPaidLink_IsReusedWithoutCreate(string status)
    {
        PaymentHarness harness = CreateHarness();
        harness
            .Client.Setup(client => client.GetPaymentLinkInformationAsync(10))
            .ReturnsAsync(Link(status));

        Result<CreatePaymentResult> result = await harness.Handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10 },
            default
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(status, result.Value?.status);
        Assert.Equal("https://pay.payos.vn/web/link-10", result.Value?.checkoutUrl);
        harness.Client.Verify(
            client => client.CreatePaymentLinkAsync(It.IsAny<PaymentData>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExistingCancelledLink_IsNeverRecreatedAndExplainsRecovery()
    {
        PaymentHarness harness = CreateHarness();
        harness
            .Client.Setup(client => client.GetPaymentLinkInformationAsync(10))
            .ReturnsAsync(Link("CANCELLED"));

        Result<CreatePaymentResult> result = await harness.Handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10 },
            default
        );

        Assert.Contains("Choose cash or cancel", result.Error?.Title);
        harness.Client.Verify(
            client => client.CreatePaymentLinkAsync(It.IsAny<PaymentData>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UnknownProviderState_IsNeverRecreated()
    {
        PaymentHarness harness = CreateHarness();
        harness
            .Client.Setup(client => client.GetPaymentLinkInformationAsync(10))
            .ReturnsAsync(Link("EXPIRED"));

        Result<CreatePaymentResult> result = await harness.Handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10 },
            default
        );

        Assert.Equal(400, result.Error?.Status);
        harness.Client.Verify(
            client => client.CreatePaymentLinkAsync(It.IsAny<PaymentData>()),
            Times.Never
        );
    }

    [Theory]
    [InlineData("401")]
    [InlineData("429")]
    [InlineData("20")]
    public async Task NonNotFoundLookupErrors_AreControlledAndNeverCreate(string code)
    {
        PaymentHarness harness = CreateHarness();
        harness
            .Client.Setup(client => client.GetPaymentLinkInformationAsync(10))
            .ThrowsAsync(new PayOSError(code, "Provider failure"));

        Result<CreatePaymentResult> result = await harness.Handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10 },
            default
        );

        Assert.Equal(400, result.Error?.Status);
        harness.Client.Verify(
            client => client.CreatePaymentLinkAsync(It.IsAny<PaymentData>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExistingLinkAmountMismatch_IsRejected()
    {
        PaymentHarness harness = CreateHarness();
        harness
            .Client.Setup(client => client.GetPaymentLinkInformationAsync(10))
            .ReturnsAsync(Link("PENDING", amount: 99));

        Result<CreatePaymentResult> result = await harness.Handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10 },
            default
        );

        Assert.Contains("does not match", result.Error?.Title);
    }

    [Fact]
    public async Task ExistingLinkOrderCodeMismatch_IsRejected()
    {
        PaymentHarness harness = CreateHarness();
        harness
            .Client.Setup(client => client.GetPaymentLinkInformationAsync(10))
            .ReturnsAsync(Link("PENDING", orderCode: 11));

        Result<CreatePaymentResult> result = await harness.Handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10 },
            default
        );

        Assert.Contains("does not match", result.Error?.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100.5)]
    public async Task InvalidAuthoritativeAmount_NeverCallsProvider(decimal total)
    {
        PaymentHarness harness = CreateHarness(total: total);

        Result<CreatePaymentResult> result = await harness.Handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10 },
            default
        );

        Assert.Equal(400, result.Error?.Status);
        harness.Client.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InformationalItems_AreOmittedWhenTheyDoNotMatchOrderTotal()
    {
        PaymentHarness harness = CreateHarness(itemPrice: 90);
        PaymentData? captured = null;
        harness
            .Client.Setup(client => client.GetPaymentLinkInformationAsync(10))
            .ThrowsAsync(new PayOSError(PayOsOrderPolicy.PaymentLinkNotFoundCode, "Not found"));
        harness
            .Client.Setup(client => client.CreatePaymentLinkAsync(It.IsAny<PaymentData>()))
            .Callback<PaymentData>(request => captured = request)
            .ReturnsAsync(Created("PENDING"));

        _ = await harness.Handler.Handle(new GetLinkPaymentQuery { OrderId = 10 }, default);

        Assert.Empty(Assert.IsType<List<ItemData>>(captured?.items));
    }

    [Fact]
    public async Task SequentialRetries_CreateAtMostOnceAndThenReuse()
    {
        PaymentHarness harness = CreateHarness();
        harness
            .Client.SetupSequence(client => client.GetPaymentLinkInformationAsync(10))
            .ThrowsAsync(new PayOSError(PayOsOrderPolicy.PaymentLinkNotFoundCode, "Not found"))
            .ReturnsAsync(Link("PENDING"));
        harness
            .Client.Setup(client => client.CreatePaymentLinkAsync(It.IsAny<PaymentData>()))
            .ReturnsAsync(Created("PENDING"));

        Result<CreatePaymentResult> first = await harness.Handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10 },
            default
        );
        Result<CreatePaymentResult> retry = await harness.Handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10 },
            default
        );

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        harness.Client.Verify(
            client => client.CreatePaymentLinkAsync(It.IsAny<PaymentData>()),
            Times.Once
        );
    }

    private static PaymentHarness CreateHarness(
        OrderStatus status = OrderStatus.Processed,
        decimal total = 100,
        decimal itemPrice = 100,
        IOrderPaymentSettings? paymentSettings = null
    )
    {
        Mock<IOrderPaymentLinkClient> client = new(MockBehavior.Strict);
        Mock<IDynamicSpecificationRepository<Order>> orders = new(MockBehavior.Strict);
        orders
            .Setup(repository =>
                repository.FindByConditionAsync(
                    It.IsAny<ISpecification<Order>>(),
                    It.IsAny<Expression<Func<Order, OrderPayment>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new OrderPayment
                {
                    Id = 10,
                    BranchId = 1,
                    Code = "ORD-1",
                    Amount = total,
                    Status = status,
                    Items =
                    [
                        new OrderPaymentItem
                        {
                            Name = "Wash",
                            Quantity = 1,
                            Amount = itemPrice,
                        },
                    ],
                }
            );
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        unitOfWork
            .Setup(work => work.DynamicReadOnlyRepository<Order>(false))
            .Returns(orders.Object);
        var handler = new GetLinkPaymentHandler(
            client.Object,
            unitOfWork.Object,
            CurrentAccount(),
            paymentSettings ?? EnabledSettings()
        );
        return new PaymentHarness(handler, client);
    }

    private static ICurrentAccount CurrentAccount() =>
        new StubCurrentAccount
        {
            Id = 99,
            Session = new UserAuth
            {
                Id = 99,
                Role = "STAFF",
                Branches = ["1"],
            },
        };

    private static IOrderPaymentSettings EnabledSettings() =>
        new StubPaymentSettings(
            true,
            "https://app.test/payment/payos-return",
            "https://app.test/payment/payos-return",
            "https://api.test/Webhook/api/CompletedOrder"
        );

    private static IOrderPaymentSettings DisabledSettings() =>
        new StubPaymentSettings(false, null, null, null);

    private static PaymentLinkInformation Link(
        string status,
        int amount = 100,
        long orderCode = 10
    ) => new("link-10", orderCode, amount, 0, amount, status, "now", [], null, null);

    private static CreatePaymentResult Created(string status) =>
        new(
            bin: string.Empty,
            accountNumber: string.Empty,
            amount: 100,
            description: "VW ORD1",
            orderCode: 10,
            currency: "VND",
            paymentLinkId: "link-10",
            status: status,
            expiredAt: null,
            checkoutUrl: "https://pay.payos.vn/web/link-10",
            qrCode: string.Empty
        );

    private sealed record PaymentHarness(
        GetLinkPaymentHandler Handler,
        Mock<IOrderPaymentLinkClient> Client
    );
}

public class PayOsWebhookTests
{
    [Fact]
    public void RealSdkAcceptsCorrectHmacAndRejectsTamperingOrWrongKey()
    {
        const string checksum = "local-test-checksum-not-a-merchant-secret";
        var data = WebhookData();
        var json = System.Text.Json.JsonSerializer.SerializeToElement(data);
        var canonical = string.Join("&", json.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal)
            .Select(x => $"{x.Name}={x.Value}"));
        var signature = Convert.ToHexString(System.Security.Cryptography.HMACSHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(checksum), System.Text.Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
        var verifier = new PayOsWebhookVerifier(new Net.payOS.PayOS("test-client", "test-api-key", checksum));
        var verified = verifier.Verify(new WebhookType("00", "success", true, data, signature));
        Assert.Equal(10, verified.orderCode);
        Assert.Equal(100, verified.amount);
        Assert.ThrowsAny<Exception>(() => verifier.Verify(new WebhookType("00", "success", true,
            WebhookData(amount: 101), signature)));
        var wrongKey = new PayOsWebhookVerifier(new Net.payOS.PayOS("test-client", "test-api-key", "different-test-key"));
        Assert.ThrowsAny<Exception>(() => wrongKey.Verify(new WebhookType("00", "success", true, data, signature)));
    }

    [Theory]
    [InlineData(false, "00", "00", 100)]
    [InlineData(true, "01", "00", 100)]
    [InlineData(true, "00", "01", 100)]
    [InlineData(true, "00", "00", 0)]
    [InlineData(true, "00", "00", -1)]
    public void MalformedOrUnsuccessfulWebhook_IsRejected(
        bool success,
        string requestCode,
        string dataCode,
        int amount
    ) => Assert.False(PayOsWebhookPolicy.IsSuccessful(success, requestCode, dataCode, amount));

    [Fact]
    public void SuccessfulWebhook_RequiresAllAuthoritySignals()
    {
        Assert.True(PayOsWebhookPolicy.IsSuccessful(true, "00", "00", 100));
    }

    [Fact]
    public void PayOsConfirmationSample_IsRecognizedExactly()
    {
        Assert.True(
            PayOsWebhookPolicy.IsConfirmationSample(123, 3000, "VQRIO123", "TF230204212323")
        );
    }

    [Theory]
    [InlineData(124, 3000, "VQRIO123", "TF230204212323")]
    [InlineData(123, 3001, "VQRIO123", "TF230204212323")]
    [InlineData(123, 3000, "VW REAL", "TF230204212323")]
    [InlineData(123, 3000, "VQRIO123", "real-reference")]
    public void RealOrderCannotMatchConfirmationSample(
        long orderCode,
        int amount,
        string description,
        string reference
    ) =>
        Assert.False(
            PayOsWebhookPolicy.IsConfirmationSample(orderCode, amount, description, reference)
        );

    [Fact]
    public async Task InvalidSignature_ReturnsBadRequestWithoutSendingCommand()
    {
        Mock<IOrderPaymentWebhookVerifier> verifier = new(MockBehavior.Strict);
        verifier.Setup(item => item.Verify(It.IsAny<WebhookType>())).Throws(new Exception());
        Mock<ISender> sender = new(MockBehavior.Strict);
        var endpoint = new CompletedOrderWebhook(verifier.Object, sender.Object);

        ActionResult<ApiResponse> result = await endpoint.HandleAsync(Webhook());

        Assert.IsType<BadRequestResult>(result.Result);
        sender.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ConfirmationSample_ReturnsOkWithoutTouchingOrder()
    {
        WebhookData sample = WebhookData(123, 3000, "VQRIO123", "TF230204212323");
        Mock<IOrderPaymentWebhookVerifier> verifier = new(MockBehavior.Strict);
        verifier.Setup(item => item.Verify(It.IsAny<WebhookType>())).Returns(sample);
        Mock<ISender> sender = new(MockBehavior.Strict);
        var endpoint = new CompletedOrderWebhook(verifier.Object, sender.Object);

        ActionResult<ApiResponse> result = await endpoint.HandleAsync(Webhook(data: sample));

        Assert.IsType<OkResult>(result.Result);
        sender.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidWebhook_SendsOnlyVerifiedCardCompletionAndReturnsOk()
    {
        WebhookData data = WebhookData(10, 100, "VW ORD1", "real-reference");
        UpdateStatusCommand? captured = null;
        Mock<IOrderPaymentWebhookVerifier> verifier = new(MockBehavior.Strict);
        verifier.Setup(item => item.Verify(It.IsAny<WebhookType>())).Returns(data);
        Mock<ISender> sender = new(MockBehavior.Strict);
        sender
            .Setup(item =>
                item.Send(It.IsAny<UpdateStatusCommand>(), It.IsAny<CancellationToken>())
            )
            .Callback<IRequest<Result>, CancellationToken>(
                (command, _) => captured = Assert.IsType<UpdateStatusCommand>(command)
            )
            .ReturnsAsync(Result.Success());
        var endpoint = new CompletedOrderWebhook(verifier.Object, sender.Object);

        ActionResult<ApiResponse> result = await endpoint.HandleAsync(Webhook(data: data));

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("10", captured?.OrderId);
        Assert.Equal(100, captured?.ExpectedPaymentAmount);
        Assert.Equal(OrderStatus.Completed, captured?.Model.Status);
        Assert.Equal(PaymentMethod.Card, captured?.Model.PaymentMethod);
        PropertyInfo marker = typeof(UpdateStatusCommand).GetProperty(
            "IsVerifiedPayOsWebhook",
            BindingFlags.Instance | BindingFlags.NonPublic
        )!;
        Assert.True((bool)marker.GetValue(captured!)!);
    }

    [Fact]
    public void PaymentLinkEndpoint_IsPostWithRouteOnlyRequest()
    {
        MethodInfo method = typeof(GetGetLinkPaymentEndpoint).GetMethod("HandleAsync")!;
        HttpPostAttribute post = Assert.IsType<HttpPostAttribute>(
            method.GetCustomAttribute(typeof(HttpPostAttribute))
        );

        Assert.Equal(Router.OrderRoute.GetLinkPayment, post.Template);
        Assert.Null(typeof(GetLinkPaymentQuery).GetProperty("ReturnUrl"));
    }

    private static WebhookType Webhook(
        bool success = true,
        string requestCode = "00",
        WebhookData? data = null
    ) => new(requestCode, "success", success, data ?? WebhookData(), "signature");

    private static WebhookData WebhookData(
        long orderCode = 10,
        int amount = 100,
        string description = "VW ORD1",
        string reference = "real-reference",
        string dataCode = "00"
    ) =>
        new(
            orderCode,
            amount,
            description,
            "12345678",
            reference,
            "2026-09-02 10:00:00",
            "VND",
            "link-10",
            dataCode,
            "success",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty
        );
}

file sealed record StubPaymentSettings(
    bool IsEnabled,
    string? ReturnUrl,
    string? CancelUrl,
    string? WebhookUrl
) : IOrderPaymentSettings;

file sealed class StubCurrentAccount : ICurrentAccount
{
    public long? Id { get; init; }

    public string? ClientIp { get; private set; }

    public UserAuth? Session { get; init; }

    public Task SetClaimPrinciple(System.Security.Claims.ClaimsPrincipal user) =>
        Task.CompletedTask;

    public void SetClientIp(Microsoft.AspNetCore.Http.HttpContext httpContext) =>
        ClientIp = httpContext.Connection.RemoteIpAddress?.ToString();
}
