using System.Text.Json;
using Application.Feature.Common.Projections.Orders;
using Application.Feature.Common.Projections.Vouchers;
using Application.Feature.Common.Validators.Vouchers;
using Application.Feature.Orders.Command.Create;
using Application.Feature.Orders.Command.Update;
using Application.Feature.Orders.Common;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Specifications;

namespace EcommerceService.Tests;

public class OrderPricingTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ManipulatedClientPrice_CannotChangePersistedOrderAmount()
    {
        const string json = """
            {
              "customerId": 1,
              "branchId": 10,
              "tariffId": 100,
              "point": 999999,
              "discountFixed": true,
              "discountValue": 999999,
              "orderItems": [
                {
                  "serviceId": 20,
                  "unitRelationId": 30,
                  "quantity": 2,
                  "price": 1,
                  "unitPrice": 1,
                  "serviceName": "tampered",
                  "unitRelationName": "tampered",
                  "processingTime": 0
                }
              ]
            }
            """;

        CreateOrderCommand command = JsonSerializer.Deserialize<CreateOrderCommand>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        )!;
        ResultAssertSuccess(
            Resolve(command.OrderItems, ValidTariff(), [ValidPricingRow(price: 125m)])
        );
        ResolvedOrderPricing pricing = Resolve(
            command.OrderItems,
            ValidTariff(),
            [ValidPricingRow(price: 125m)]
        ).Value!;
        OrderPriceSummary totals = OrderPriceCalculator.Calculate(
            pricing.Items,
            discountFixed: false,
            discountValue: 0,
            vatPercent: 0
        ).Value!;

        Order order = command.ToEntity(7, 0, pricing, totals, voucher: null);

        Assert.Null(typeof(OrderItemSelectionModel).GetProperty("Price"));
        Assert.Equal(125m, order.OrderItems.Single().Price);
        Assert.Equal(250m, order.Amount);
        Assert.Equal(250m, order.Total);
        Assert.Equal(0, order.Point);
    }

    [Fact]
    public void Resolver_UsesTariffPriceAndAuthoritativeSnapshots()
    {
        var row = ValidPricingRow(price: 175m) with
        {
            UnitRelationPrice = 120m,
            ServiceName = "Premium Wash",
            UnitRelationName = "Kilogram",
            ProcessingTime = 45m,
        };

        ResolvedOrderItem item = Resolve(ValidItems(), ValidTariff(), [row]).Value!.Items.Single();

        Assert.Equal(175m, item.Price);
        Assert.Equal(120m, item.UnitPrice);
        Assert.Equal("Premium Wash", item.ServiceName);
        Assert.Equal("Kilogram", item.UnitRelationName);
        Assert.Equal(45m, item.ProcessingTime);
    }

    [Fact]
    public void Resolver_RejectsMissingServiceTariffCombination()
    {
        List<OrderItemSelectionModel> items =
        [
            new() { ServiceId = 999, UnitRelationId = 30, Quantity = 1 },
        ];

        Assert.True(Resolve(items, ValidTariff(), [ValidPricingRow()]).IsFailure);
    }

    [Fact]
    public void Resolver_RejectsMissingUnitRelationCombination()
    {
        List<OrderItemSelectionModel> items =
        [
            new() { ServiceId = 20, UnitRelationId = 999, Quantity = 1 },
        ];

        Assert.True(Resolve(items, ValidTariff(), [ValidPricingRow()]).IsFailure);
    }

    [Fact]
    public void Resolver_RejectsUnitRelationThatBelongsToAnotherService()
    {
        ServiceTariffPricingSnapshot row = ValidPricingRow() with
        {
            UnitRelationServiceId = 999,
        };

        Assert.True(Resolve(ValidItems(), ValidTariff(), [row]).IsFailure);
    }

    [Fact]
    public void Resolver_RejectsCrossBranchTariff()
    {
        TariffPricingSnapshot tariff = ValidTariff() with { BranchId = 99 };

        Assert.True(Resolve(ValidItems(), tariff, [ValidPricingRow()]).IsFailure);
    }

    [Fact]
    public void Resolver_RejectsCrossBranchService()
    {
        ServiceTariffPricingSnapshot row = ValidPricingRow() with { ServiceBranchId = 99 };

        Assert.True(Resolve(ValidItems(), ValidTariff(), [row]).IsFailure);
    }

    [Fact]
    public void Resolver_RejectsDisabledTariff()
    {
        Assert.True(
            Resolve(
                ValidItems(),
                ValidTariff() with { Disable = true },
                [ValidPricingRow()]
            ).IsFailure
        );
    }

    [Fact]
    public void Resolver_RejectsInactiveTariff()
    {
        Assert.True(
            Resolve(
                ValidItems(),
                ValidTariff() with { Status = ActivationStatus.Inactive },
                [ValidPricingRow()]
            ).IsFailure
        );
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(-2, -1)]
    public void Resolver_RejectsTariffOutsideValidityWindow(int startOffsetDays, int endOffsetDays)
    {
        TariffPricingSnapshot tariff = ValidTariff() with
        {
            StartAt = Now.AddDays(startOffsetDays),
            EndAt = Now.AddDays(endOffsetDays),
        };

        Assert.True(Resolve(ValidItems(), tariff, [ValidPricingRow()]).IsFailure);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Resolver_RejectsInactiveServiceOrUnitRelation(
        bool inactiveService,
        bool inactiveUnitRelation
    )
    {
        ServiceTariffPricingSnapshot row = ValidPricingRow() with
        {
            ServiceStatus = inactiveService
                ? ActivationStatus.Inactive
                : ActivationStatus.Active,
            UnitRelationStatus = inactiveUnitRelation
                ? ActivationStatus.Inactive
                : ActivationStatus.Active,
        };

        Assert.True(Resolve(ValidItems(), ValidTariff(), [row]).IsFailure);
    }

    [Fact]
    public void Resolver_RejectsNonPositiveQuantity()
    {
        List<OrderItemSelectionModel> items =
        [
            new() { ServiceId = 20, UnitRelationId = 30, Quantity = 0 },
        ];

        Assert.True(Resolve(items, ValidTariff(), [ValidPricingRow()]).IsFailure);
        Assert.False(
            new CreateOrderCommandValidator()
                .Validate(ValidCreateCommand(items))
                .IsValid
        );
    }

    [Fact]
    public void Validators_RejectDuplicateServiceAndUnitSelection()
    {
        List<OrderItemSelectionModel> items =
        [
            new() { ServiceId = 20, UnitRelationId = 30, Quantity = 1 },
            new() { ServiceId = 20, UnitRelationId = 30, Quantity = 2 },
        ];

        Assert.False(new CreateOrderCommandValidator().Validate(ValidCreateCommand(items)).IsValid);
        Assert.False(
            new UpdateOrderValidator()
                .Validate(
                    new UpdateOrderCommand
                    {
                        OrderId = 1,
                        Model = new UpdateOrderModel { TariffId = 100, OrderItems = items },
                    }
                )
                .IsValid
        );
    }

    [Fact]
    public void Validators_RejectNullModelsAndItemCollectionsWithoutThrowing()
    {
        CreateOrderCommand create = ValidCreateCommand(ValidItems());
        create.OrderItems = null!;
        var updateWithoutModel = new UpdateOrderCommand { OrderId = 1, Model = null! };
        var updateWithoutItems = new UpdateOrderCommand
        {
            OrderId = 1,
            Model = new UpdateOrderModel { TariffId = 100, OrderItems = null! },
        };

        Assert.False(new CreateOrderCommandValidator().Validate(create).IsValid);
        Assert.False(new UpdateOrderValidator().Validate(updateWithoutModel).IsValid);
        Assert.False(new UpdateOrderValidator().Validate(updateWithoutItems).IsValid);
    }

    [Fact]
    public void Calculator_AppliesFixedDiscountAsMoney()
    {
        OrderPriceSummary totals = Calculate(price: 100m, quantity: 2, true, 25m).Value!;

        Assert.Equal(200m, totals.Amount);
        Assert.Equal(25m, totals.DiscountAmount);
        Assert.Equal(175m, totals.Total);
    }

    [Fact]
    public void Calculator_AppliesNonFixedDiscountAsPercentage()
    {
        OrderPriceSummary totals = Calculate(price: 100m, quantity: 2, false, 25m).Value!;

        Assert.Equal(50m, totals.DiscountAmount);
        Assert.Equal(150m, totals.Total);
    }

    [Fact]
    public void Calculator_RejectsPercentageAboveOneHundred()
    {
        Assert.True(Calculate(100m, 1, false, 101m).IsFailure);
    }

    [Fact]
    public void Calculator_RejectsFixedDiscountThatWouldCreateNegativeTotal()
    {
        Assert.True(Calculate(100m, 1, true, 101m).IsFailure);
    }

    [Fact]
    public void Calculator_RejectsDecimalOverflow()
    {
        Assert.True(Calculate(decimal.MaxValue, 2, true, 0m).IsFailure);
    }

    [Fact]
    public void CreateOrder_UsesVoucherDiscountFromServerSnapshot()
    {
        const string json = """
            {
              "customerId": 1,
              "branchId": 10,
              "tariffId": 100,
              "discountFixed": false,
              "discountValue": 99,
              "orderItems": [{"serviceId":20,"unitRelationId":30,"quantity":1}]
            }
            """;
        CreateOrderCommand command = JsonSerializer.Deserialize<CreateOrderCommand>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        )!;
        ResolvedOrderPricing pricing = Resolve(
            command.OrderItems,
            ValidTariff(),
            [ValidPricingRow(price: 200m)]
        ).Value!;
        var voucher = new VoucherRedemption
        {
            VoucherId = 8,
            Code = "DB-VOUCHER",
            DiscountFixed = true,
            DiscountValue = 40m,
        };
        OrderPriceSummary totals = OrderPriceCalculator.Calculate(
            pricing.Items,
            voucher.DiscountFixed,
            voucher.DiscountValue,
            0
        ).Value!;

        Order order = command.ToEntity(7, 0, pricing, totals, voucher);

        Assert.True(order.DiscountFixed);
        Assert.Equal(40m, order.DiscountValue);
        Assert.Equal(160m, order.Total);
        Assert.Equal(8, order.VoucherId);
    }

    [Fact]
    public void UpdateOrder_RepricesItemsFromServerAndDisablesUnverifiedPoints()
    {
        var order = new Order(
            branchId: 10,
            staffId: 7,
            code: "OD000001",
            amount: 1m,
            total: 1m,
            status: OrderStatus.Pending,
            discountFixed: false,
            discountValue: 10m,
            point: 999m,
            tariffId: 100
        );
        var model = new UpdateOrderModel { TariffId = 100, OrderItems = ValidItems(quantity: 2) };
        ResolvedOrderPricing pricing = Resolve(
            model.OrderItems,
            ValidTariff(),
            [ValidPricingRow(price: 200m)]
        ).Value!;
        OrderPriceSummary totals = OrderPriceCalculator.Calculate(
            pricing.Items,
            order.DiscountFixed,
            order.DiscountValue,
            order.Vat
        ).Value!;

        order.FromUpdateModel(model, pricing, totals);

        Assert.Equal(200m, order.OrderItems.Single().Price);
        Assert.Equal(400m, order.Amount);
        Assert.Equal(360m, order.Total);
        Assert.Equal(0, order.Point);
    }

    [Fact]
    public void VoucherEligibility_AcceptsAssignedUnusedActiveVoucher()
    {
        Voucher voucher = ValidVoucher();

        Assert.True(VoucherEligibility.ForCustomer("SAVE", 1, Now).Compile()(voucher));
    }

    [Fact]
    public void VoucherEligibility_RejectsVoucherAssignedToAnotherCustomer()
    {
        Voucher voucher = ValidVoucher(customerId: 2);

        Assert.False(VoucherEligibility.ForCustomer("SAVE", 1, Now).Compile()(voucher));
    }

    [Fact]
    public void VoucherEligibility_RejectsUsedVoucher()
    {
        Voucher voucher = ValidVoucher(isUsed: true);

        Assert.False(VoucherEligibility.ForCustomer("SAVE", 1, Now).Compile()(voucher));
    }

    [Theory]
    [InlineData(ActivationStatus.Inactive, -1, 1)]
    [InlineData(ActivationStatus.Active, 1, 2)]
    [InlineData(ActivationStatus.Active, -2, -1)]
    public void VoucherEligibility_RejectsInactiveOrOutOfWindowVoucher(
        ActivationStatus status,
        int startOffsetDays,
        int endOffsetDays
    )
    {
        Voucher voucher = ValidVoucher();
        voucher.Status = status;
        voucher.StartAt = Now.AddDays(startOffsetDays);
        voucher.EndAt = Now.AddDays(endOffsetDays);

        Assert.False(VoucherEligibility.ForCustomer("SAVE", 1, Now).Compile()(voucher));
    }

    [Fact]
    public void VoucherValidator_AppliesPercentageLimitOnlyWhenNotFixed()
    {
        var percentage = ValidVoucherModel(discountFixed: false, discountValue: 101m);
        var fixedAmount = ValidVoucherModel(discountFixed: true, discountValue: 101m);
        var validator = new VoucherValidator(null!, null!);

        Assert.False(validator.Validate(percentage).IsValid);
        Assert.True(validator.Validate(fixedAmount).IsValid);
    }

    private static Contracts.ApiWrapper.Result<ResolvedOrderPricing> Resolve(
        IReadOnlyCollection<OrderItemSelectionModel> items,
        TariffPricingSnapshot tariff,
        IReadOnlyCollection<ServiceTariffPricingSnapshot> rows
    ) => OrderPricingResolver.Resolve(10, items, tariff, rows, Now);

    private static Contracts.ApiWrapper.Result<OrderPriceSummary> Calculate(
        decimal price,
        int quantity,
        bool discountFixed,
        decimal discountValue
    ) =>
        OrderPriceCalculator.Calculate(
            [
                new ResolvedOrderItem
                {
                    ServiceId = 20,
                    UnitRelationId = 30,
                    Quantity = quantity,
                    Price = price,
                    UnitPrice = price,
                    ServiceName = "Wash",
                    UnitRelationName = "Kg",
                },
            ],
            discountFixed,
            discountValue,
            0
        );

    private static void ResultAssertSuccess(
        Contracts.ApiWrapper.Result<ResolvedOrderPricing> result
    ) => Assert.True(result.IsSuccess, result.Error?.Title);

    private static CreateOrderCommand ValidCreateCommand(List<OrderItemSelectionModel> items) =>
        new()
        {
            CustomerId = 1,
            BranchId = 10,
            TariffId = 100,
            OrderItems = items,
        };

    private static List<OrderItemSelectionModel> ValidItems(int quantity = 1) =>
        [
            new() { ServiceId = 20, UnitRelationId = 30, Quantity = quantity },
        ];

    private static TariffPricingSnapshot ValidTariff() =>
        new()
        {
            Id = 100,
            BranchId = 10,
            Status = ActivationStatus.Active,
            Disable = false,
            StartAt = Now.AddDays(-1),
            EndAt = Now.AddDays(1),
        };

    private static ServiceTariffPricingSnapshot ValidPricingRow(decimal price = 100m) =>
        new()
        {
            TariffId = 100,
            ServiceId = 20,
            UnitRelationId = 30,
            Price = price,
            ServiceName = "Wash",
            ServiceBranchId = 10,
            ServiceDisable = false,
            ServiceStatus = ActivationStatus.Active,
            UnitRelationServiceId = 20,
            UnitRelationName = "Kg",
            UnitRelationPrice = 90m,
            ProcessingTime = 30m,
            UnitRelationStatus = ActivationStatus.Active,
        };

    private static Voucher ValidVoucher(long customerId = 1, bool isUsed = false) =>
        new()
        {
            Code = "SAVE",
            Title = "Save",
            Barcode = "barcode",
            DiscountFixed = false,
            DiscountValue = 10m,
            Status = ActivationStatus.Active,
            StartAt = Now.AddDays(-1),
            EndAt = Now.AddDays(1),
            VoucherCustomers =
            [
                new VoucherCustomer { CustomerId = customerId, IsUsed = isUsed },
            ],
        };

    private static VoucherModel ValidVoucherModel(bool discountFixed, decimal discountValue) =>
        new()
        {
            Title = "Save",
            ImgUrl = "voucher.png",
            DiscountFixed = discountFixed,
            DiscountValue = discountValue,
            StartAt = Now.AddDays(-1),
            EndAt = Now.AddDays(1),
        };
}
