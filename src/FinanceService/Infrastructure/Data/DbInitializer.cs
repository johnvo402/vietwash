using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;
using Microsoft.EntityFrameworkCore;
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
            // Danh sách chuẩn cần có
            List<FundBehavior> desired = InitializeFundBehaviorAsync();

            // Lấy các ID đã có trong DB (chỉ các bản ghi generate = true)
            var existingIds = await unitOfWork
                .Repository<FundBehavior>()
                .QueryAsync(x => x.Generate == true)
                .Select(x => x.Id)
                .ToListAsync();

            // Chỉ thêm những bản ghi còn thiếu
            var toInsert = desired.Where(x => !existingIds.Contains(x.Id)).ToList();

            if (toInsert.Count > 0)
            {
                logger.Information(
                    "Bắt đầu khởi tạo mới {Count} hành vi quỹ: {Ids}",
                    toInsert.Count,
                    string.Join(", ", toInsert.Select(i => i.Id))
                );

                foreach (var item in toInsert)
                    await unitOfWork.Repository<FundBehavior>().AddAsync(item);

                await unitOfWork.SaveAsync();
                logger.Information("Hoàn tất khởi tạo dữ liệu hành vi quỹ mới.");
            }
            else
            {
                logger.Information("Dữ liệu hành vi quỹ đã đầy đủ. Không có bản ghi mới cần thêm.");
            }

            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.Error(ex, "Lỗi khởi tạo dữ liệu hành vi quỹ: {Message}", ex.Message);
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
            new FundBehavior(
                new FundName
                {
                    vi = "Sửa chửa/Bảo hành thiết bị",
                    en = "Equipment Repair/Warranty",
                },
                FundType.Spend,
                generate: true,
                auto: false
            )
            {
                Id = 7,
            },
        };

        return danhSachHanhViQuy;
    }
}
