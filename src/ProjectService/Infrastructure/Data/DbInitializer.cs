using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Branches;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Warehouses;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.Data;

public class DbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider provider,
        CancellationToken cancellationToken = default
    )
    {
        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        var logger = provider.GetRequiredService<ILogger>();
        using var dbTransaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            // Initialize Branches
            if (!await unitOfWork.Repository<Branch>().AnyAsync())
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu chi nhánh...");

                await InitializeBranchesAsync(unitOfWork, cancellationToken);

                logger.Information("Hoàn tất khởi tạo dữ liệu chi nhánh...");
            }
            else
            {
                logger.Information("Dữ liệu chi nhánh đã tồn tại, bỏ qua khởi tạo.");
            }

            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.Error("Lỗi xảy ra trong khi khởi tạo dữ liệu chi nhánh: {Message}", ex.Message);
            throw;
        }
    }

    private static async Task InitializeBranchesAsync(
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken
    )
    {
        var branches = new List<Branch>
        {
            new Branch(
                name: "Chi nhánh Cái Răng",
                code: "CNCR",
                main: false,
                status: ActivationStatus.Active,
                email: "ct@laundry.com",
                phoneCode: null,
                phoneNumber: null,
                addressName: null,
                communeName: null,
                communeCode: null,
                districtName: null,
                districtCode: null,
                provinceName: null,
                provinceCode: null,
                street: null
            )
            {
                Disable = false,
                Id = 1, // Ensure Id is set
                PublicId = Ulid.NewUlid(), // Generate PublicId
                // PublicId, CreatedAt, CreatedBy, Version handled by AggregateRoot
            },
            new Branch(
                name: "Chi nhánh Bình Thủy",
                code: "CNBT",
                main: false,
                status: ActivationStatus.Active,
                email: "ct@laundry.com",
                phoneCode: null,
                phoneNumber: null,
                addressName: null,
                communeName: null,
                communeCode: null,
                districtName: null,
                districtCode: null,
                provinceName: null,
                provinceCode: null,
                street: null
            )
            {
                Disable = false,
                Id = 2, // Ensure Id is set
                PublicId = Ulid.NewUlid(), // Generate PublicId
            },
            new Branch(
                name: "Chi nhánh Ô Môn",
                code: "CNOM",
                main: false,
                status: ActivationStatus.Active,
                email: "ct@laundry.com",
                phoneCode: null,
                phoneNumber: null,
                addressName: null,
                communeName: null,
                communeCode: null,
                districtName: null,
                districtCode: null,
                provinceName: null,
                provinceCode: null,
                street: null
            )
            {
                Disable = false,
                Id = 3, // Ensure Id is set
                PublicId = Ulid.NewUlid(), // Generate PublicId
            },
            new Branch(
                name: "Chi nhánh Ninh Kiều",
                code: "CNNK",
                main: true,
                status: ActivationStatus.Active,
                email: "ct@laundry.com",
                phoneCode: null,
                phoneNumber: null,
                addressName: null,
                communeName: null,
                communeCode: null,
                districtName: null,
                districtCode: null,
                provinceName: null,
                provinceCode: null,
                street: null
            )
            {
                Disable = false,
                Id = 4, // Ensure Id is set
                PublicId = Ulid.NewUlid(), // Generate PublicId
            },
        };
        foreach (var branch in branches)
        {
            // Create and add default warehouse
            var warehouse = new Warehouse
            {
                Name = $"{branch.Name} kho",
                Code = $"WH-{branch.Code}",
                Description = $"Kho mặc định được tạo cùng chi nhánh {branch.Name}.",
                BranchId = branch.Id,
                ReorderLevel = 0,
                Status = ActivationStatus.Active,
            };
            branch.Warehouses.Add(warehouse);
            branch.CreateEvent();
            await unitOfWork.Repository<Branch>().AddAsync(branch, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
        }
    }
}
