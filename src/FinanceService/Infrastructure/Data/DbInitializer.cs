using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.Data;

public class FundName
{
    public string en { get; set; } = default!;
    public string vi { get; set; } = default!;
}

public class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider provider)
    {
        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        var logger = provider.GetRequiredService<ILogger>();
        using var dbTransaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            if (
                (
                    await unitOfWork
                        .Repository<FundBehavior>()
                        .FindByConditionAsync(x => x.Generate == true)
                ) == null
            )
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu hành vi quỹ...");

                List<FundBehavior> danhSachHanhViQuy = InitializeFundBehaviorAsync();

                foreach (var hanhViQuy in danhSachHanhViQuy)
                {
                    await unitOfWork.Repository<FundBehavior>().AddAsync(hanhViQuy);
                    await unitOfWork.SaveAsync();
                }

                logger.Information("Hoàn tất khởi tạo dữ liệu hành vi quỹ...");
            }
            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.Information("Lỗi xảy ra trong khi khởi tạo dữ liệu hành vi quỹ: {message}", ex);
            throw;
        }
    }

    private static List<FundBehavior> InitializeFundBehaviorAsync()
    {
        List<FundBehavior> danhSachHanhViQuy = new()
        {
            new FundBehavior(
                new FundName { vi = "Đơn hàng dịch vụ", en = "Service Order" },
                FundType.Income,
                generate: true,
                auto: true
            )
            {
                Id = 1,
            },
            new FundBehavior(
                new FundName { vi = "Hủy đơn dịch vụ", en = "Cancel Service Order" },
                FundType.Spend,
                generate: true,
                auto: true
            )
            {
                Id = 2,
            },
            new FundBehavior(
                new FundName { vi = "Phiếu chi nhập hàng", en = "Stock Import" },
                FundType.Spend,
                generate: true,
                auto: true
            )
            {
                Id = 3,
            },
            new FundBehavior(
                new FundName { vi = "Phiếu thu xuất hàng", en = "Stock Export" },
                FundType.Income,
                generate: true,
                auto: true
            )
            {
                Id = 4,
            },
            new FundBehavior(
                new FundName { vi = "Thu khác", en = "Income other" },
                FundType.Income,
                generate: true,
                auto: false
            )
            {
                Id = 5,
            },
            new FundBehavior(
                new FundName { vi = "Chi khác", en = "Spend other" },
                FundType.Spend,
                generate: true,
                auto: false
            )
            {
                Id = 6,
            },
        };

        return danhSachHanhViQuy;
    }
}
