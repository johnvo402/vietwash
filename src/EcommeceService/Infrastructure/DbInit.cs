using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Contracts.Dtos.Models;
using Contracts.Dtos.Requests;
using Contracts.Utils;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Enums;
using Domain.Aggregates.Services.Specifications;
using Domain.Aggregates.Suppliers;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Domain.Aggregates.Vouchers;
using Infrastructure.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Specification;

namespace Infrastructure.Data;

public class DbInitializer
{
    private static readonly DateTimeOffset StartDate = new DateTimeOffset(
        2024,
        10,
        1,
        0,
        0,
        0,
        TimeSpan.Zero
    );
    private static readonly DateTimeOffset EndDate = new DateTimeOffset(
        2025,
        8,
        6,
        18,
        14,
        0,
        TimeSpan.Zero
    );
    private static readonly TimeSpan TotalPeriod = EndDate - StartDate;

    public static async Task InitializeAsync(
        IServiceProvider provider,
        CancellationToken cancellationToken = default
    )
    {
        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        var encryption = provider.GetRequiredService<IEncryptionService>();
        var qrGenerator = provider.GetRequiredService<IQrGenerator>();
        var logger = provider.GetRequiredService<ILogger>();
        var media = provider.GetRequiredService<IMediaUpdateService>();

        using var dbTransaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            if (!await unitOfWork.Repository<Unit>().AnyAsync(cancellationToken: cancellationToken))
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu đơn vị tính...");
                await InitializeUnitsAsync(unitOfWork, cancellationToken);
                logger.Information("Hoàn tất khởi tạo dữ liệu đơn vị tính.");
            }

            if (
                !await unitOfWork
                    .Repository<Category>()
                    .AnyAsync(cancellationToken: cancellationToken)
            )
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu danh mục...");
                await InitializeCategoriesAsync(unitOfWork, cancellationToken);
                logger.Information("Hoàn tất khởi tạo dữ liệu danh mục.");
            }

            if (
                !await unitOfWork
                    .Repository<Supplier>()
                    .AnyAsync(cancellationToken: cancellationToken)
            )
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu nhà cung cấp...");
                await InitializeSuppliersAsync(unitOfWork, cancellationToken);
                logger.Information("Hoàn tất khởi tạo dữ liệu nhà cung cấp.");
            }

            if (
                !await unitOfWork
                    .Repository<Service>()
                    .AnyAsync(cancellationToken: cancellationToken)
            )
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu dịch vụ...");
                await InitializeServicesAsync(unitOfWork, logger, media, cancellationToken);
                logger.Information("Hoàn tất khởi tạo dữ liệu dịch vụ.");
            }

            if (
                !await unitOfWork
                    .Repository<Tariff>()
                    .AnyAsync(cancellationToken: cancellationToken)
            )
            {
                logger.Information("Bắt đầu khởi tạo biểu phí...");
                await InitializeTariffsAsync(unitOfWork, logger, cancellationToken);
                logger.Information("Hoàn tất khởi tạo biểu phí.");
            }

            if (
                !await unitOfWork
                    .Repository<Voucher>()
                    .AnyAsync(cancellationToken: cancellationToken)
            )
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu voucher...");
                await InitVouchersAsync(unitOfWork, qrGenerator, logger, cancellationToken);
                logger.Information("Hoàn tất khởi tạo dữ liệu voucher.");
            }

            if (
                !await unitOfWork.Repository<Order>().AnyAsync(cancellationToken: cancellationToken)
            )
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu đơn hàng...");
                await InitializeOrdersAsync(
                    unitOfWork,
                    logger,
                    cancellationToken,
                    encryption,
                    qrGenerator
                );
                logger.Information("Hoàn tất khởi tạo dữ liệu đơn hàng.");
            }

            if (
                !await unitOfWork
                    .Repository<BranchProduct>()
                    .AnyAsync(cancellationToken: cancellationToken)
            )
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu sản phẩm chi nhánh...");
                await InitializeBranchProductsAsync(unitOfWork, logger, cancellationToken);
                logger.Information("Hoàn tất khởi tạo dữ liệu sản phẩm chi nhánh.");
            }

            if (
                !await unitOfWork
                    .Repository<InventoryDocument>()
                    .AnyAsync(cancellationToken: cancellationToken)
            )
            {
                logger.Information("Bắt đầu khởi tạo phiếu nhập kho...");
                await InitializeInventoryDocumentsAsync(unitOfWork, logger, cancellationToken);
                logger.Information("Hoàn tất khởi tạo phiếu nhập kho.");
            }

            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            logger.Error(ex, "Lỗi xảy ra trong khi khởi tạo dữ liệu: {Message}", ex.Message);
            throw;
        }
    }

    private static Expression<Func<User, ListIds>> SelectOnlyId() =>
        user => new ListIds { Id = user.Id };

    private static DateTimeOffset GenerateDistributedDate(int index, int totalCount)
    {
        if (totalCount <= 1)
            return StartDate;

        // Giữ fraction trong [0, 1]
        double fraction = Math.Clamp((double)index / (totalCount - 1), 0.0, 1.0);

        // Tính ticks và đảm bảo không vượt quá phạm vi hợp lệ
        long maxTicks = DateTimeOffset.MaxValue.Ticks;
        long minTicks = DateTimeOffset.MinValue.Ticks;

        long ticks = StartDate.Ticks + (long)(fraction * TotalPeriod.Ticks);
        ticks = Math.Clamp(ticks, minTicks, maxTicks);

        return new DateTimeOffset(ticks, TimeSpan.Zero);
    }

    private static async Task InitializeUnitsAsync(
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken
    )
    {
        var units = new List<(string Name, string Description)>
        {
            ("Kg", "Khối lượng tính bằng kilogam, dùng cho dịch vụ giặt hoặc sấy quần áo."),
            ("Bộ", "Đơn vị tính cho một bộ đồ hoặc chăn mền."),
            ("Mét", "Đơn vị tính cho thảm hoặc các vật liệu dạng mét vuông."),
            ("Lít", "Đơn vị tính cho nước giặt, nước xả hoặc chất tẩy rửa."),
            ("Đôi", "Đơn vị tính cho giày hoặc các vật phẩm theo cặp."),
            ("Hộp", "Đơn vị tính cho bột giặt hoặc viên giặt đóng gói."),
            ("Thùng", "Đơn vị tính cho các sản phẩm đóng thùng lớn."),
            ("Chai", "Đơn vị tính cho chai nước giặt, nước xả hoặc chất tẩy rửa."),
        };

        for (int i = 0; i < units.Count; i++)
        {
            var (name, description) = units[i];
            var unit = new Unit(name: name, status: ActivationStatus.Active)
            {
                CreatedAt = GenerateDistributedDate(i, units.Count),
            };
            await unitOfWork.Repository<Unit>().AddAsync(unit, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
        }
    }

    private static async Task InitializeCategoriesAsync(
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken
    )
    {
        var categories = new List<(string Name, string Code)>
        {
            ("Giặt", "DM_GIAT"),
            ("Ủi", "DM_UI"),
            ("Sấy", "DM_SAY"),
            ("Vệ Sinh", "DM_VESINH"),
            ("Combo", "DM_COMBO"),
            ("Giặt Đặc Biệt", "DM_GIATDB"),
            ("Nước giặt", "DM_NUOCGIAT"),
            ("Nước xả", "DM_NUOCXA"),
            ("Nước vệ sinh", "DM_NUOCVESINH"),
        };

        for (int i = 0; i < categories.Count; i++)
        {
            var (name, code) = categories[i];
            var category = new Category(
                name: name,
                parentId: null,
                status: ActivationStatus.Active,
                code: code
            )
            {
                Disabled = false,
                Path = code.ToLowerInvariant(),
                CreatedAt = GenerateDistributedDate(i, categories.Count),
            };
            await unitOfWork.Repository<Category>().AddAsync(category, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
        }
    }

    private static async Task InitializeSuppliersAsync(
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken
    )
    {
        var suppliers = new List<Supplier>
        {
            new Supplier(
                name: "Điện Máy Xanh",
                code: Generator.GenerateCode("DMX", 6),
                status: ActivationStatus.Active,
                email: "cskh@thegioididong.com",
                address: "172B Đường 3/2, Phường Hưng Lợi, Quận Ninh Kiều, Thành phố Cần Thơ",
                phone: "02838125960",
                description: "Siêu thị Điện máy XANH tại Cần Thơ cung cấp sản phẩm chính hãng đa dạng từ điện lạnh, gia dụng đến điện tử, viễn thông."
            )
            {
                Disable = false,
                CreatedAt = GenerateDistributedDate(0, 3),
            },
            new Supplier(
                name: "Siêu Thị Điện Máy Chợ Lớn",
                code: Generator.GenerateCode("STDMCL", 6),
                status: ActivationStatus.Active,
                email: "dienmaycantho2@dienmaycholon.com.vn",
                address: "161 Đường 3/2, Phường Hưng Lợi, Quận Ninh Kiều, TP.Cần Thơ",
                phone: "02839505060",
                description: "Siêu Thị Điện Máy Chợ Lớn chi nhánh Ninh Kiều, Cần Thơ với không gian trưng bày rộng lớn và đa dạng sản phẩm."
            )
            {
                Disable = false,
                CreatedAt = GenerateDistributedDate(1, 3),
            },
            new Supplier(
                name: "Siêu Thị GO! VIETNAM",
                code: Generator.GenerateCode("STG", 6),
                status: ActivationStatus.Active,
                email: "crv.dvkh@vn.centralretail.com",
                address: "Lô số 1, KDC Hưng Phú 1, Phường Hưng Phú, Quận Cái Răng, TP. Cần Thơ",
                phone: "02923737575",
                description: "Đơn vị tiên phong cung cấp và setup hệ thống giặt là cho khách sạn, xưởng giặt là, bệnh viện."
            )
            {
                Disable = false,
                CreatedAt = GenerateDistributedDate(2, 3),
            },
        };

        foreach (var supplier in suppliers)
        {
            await unitOfWork.Repository<Supplier>().AddAsync(supplier, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
        }
    }

    private static async Task InitializeServicesAsync(
        IUnitOfWork unitOfWork,
        ILogger logger,
        IMediaUpdateService media,
        CancellationToken cancellationToken
    )
    {
        var categories = (
            await unitOfWork.Repository<Category>().ListAsync(cancellationToken)
        ).ToDictionary(c => c.Name, c => c.Id);
        var user = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(
                new ListUserSpecification([ROLE.ADMIN]),
                cancellationToken: cancellationToken
            );
        var units = (await unitOfWork.Repository<Unit>().ListAsync(cancellationToken)).ToDictionary(
            u => u.Name,
            u => u
        );

        if (user == null)
            throw new InvalidOperationException("Admin user not found.");
        if (!categories.Any() || !units.Any())
            throw new InvalidOperationException("Missing required Category or Unit data.");

        var servicesToSeed = new (
            string Name,
            string Description,
            TypeStatus Type,
            string Category,
            (
                string Unit,
                bool IsBaseUnit,
                decimal Multiple,
                decimal Price,
                decimal ProcessingTime
            )[] Units
        )[]
        {
            (
                "Combo Giặt Sấy Quần Áo",
                "Dịch vụ combo giặt và sấy quần áo tiện lợi.",
                TypeStatus.Combo,
                "Combo",
                new[] { ("Kg", true, 1m, 30000m, 20m) }
            ),
            (
                "Combo Giặt Sấy Ủi Quần Áo",
                "Combo toàn diện: giặt, sấy và ủi quần áo.",
                TypeStatus.Combo,
                "Combo",
                new[] { ("Kg", true, 1m, 45000m, 30m) }
            ),
            (
                "Combo Giặt Sấy Ủi",
                "Gói giặt sấy ủi cho đồ dùng hàng ngày.",
                TypeStatus.Combo,
                "Combo",
                new[] { ("Bộ", true, 1m, 40000m, 60m) }
            ),
            (
                "Combo Tuần Tháng",
                "Gói combo cho giặt đồ định kỳ tuần/tháng.",
                TypeStatus.Combo,
                "Combo",
                new[] { ("Kg", true, 1m, 35000m, 20m) }
            ),
            (
                "Giặt Chăn Mền Dày",
                "Làm sạch sâu chăn mền dày và nặng.",
                TypeStatus.SingleService,
                "Giặt",
                new[] { ("Bộ", true, 1m, 50000m, 60m) }
            ),
            (
                "Giặt Đồ Trẻ Em",
                "Giặt đồ nhẹ nhàng an toàn cho trẻ nhỏ.",
                TypeStatus.SingleService,
                "Giặt",
                new[] { ("Kg", true, 1m, 25000m, 20m) }
            ),
            (
                "Giặt Hấp Váy Dạ Hội",
                "Giặt hấp cao cấp cho váy dạ hội và đồ cao cấp.",
                TypeStatus.SingleService,
                "Giặt Đặc Biệt",
                new[] { ("Bộ", true, 1m, 80000m, 90m) }
            ),
            (
                "Giặt Khô Áo Vest",
                "Giặt khô chuyên dụng cho áo vest.",
                TypeStatus.SingleService,
                "Giặt Đặc Biệt",
                new[] { ("Bộ", true, 1m, 60000m, 60m) }
            ),
            (
                "Giặt Nước Quần Jeans",
                "Giặt giữ màu và chất lượng cho quần jeans.",
                TypeStatus.SingleService,
                "Giặt",
                new[] { ("Bộ", true, 1m, 30000m, 30m) }
            ),
            (
                "Giặt Quần Áo Trắng",
                "Tẩy trắng, làm sạch sâu cho quần áo trắng.",
                TypeStatus.SingleService,
                "Giặt",
                new[] { ("Kg", true, 1m, 28000m, 20m) }
            ),
            (
                "Giặt Tẩy Vết Bẩn Cứng Đầu",
                "Tẩy vết bẩn khó xử lý trên quần áo.",
                TypeStatus.SingleService,
                "Giặt Đặc Biệt",
                new[] { ("Bộ", true, 1m, 40000m, 30m) }
            ),
            (
                "Giặt Thảm",
                "Làm sạch thảm trải sàn tại nhà hoặc văn phòng.",
                TypeStatus.SingleService,
                "Giặt",
                new[] { ("Mét", true, 1m, 35000m, 30m) }
            ),
            (
                "Sấy Khô Chăn Grab",
                "Sấy khô chăn mền nhanh chóng và an toàn.",
                TypeStatus.SingleService,
                "Sấy",
                new[] { ("Bộ", true, 1m, 25000m, 30m) }
            ),
            (
                "Sấy Khô Quần Áo",
                "Sấy khô quần áo chống ẩm mốc.",
                TypeStatus.SingleService,
                "Sấy",
                new[] { ("Kg", true, 1m, 20000m, 15m) }
            ),
            (
                "Ủi Áo Sơ Mi",
                "Ủi áo sơ mi chuyên nghiệp, gọn gàng.",
                TypeStatus.SingleService,
                "Ủi",
                new[] { ("Bộ", true, 1m, 15000m, 15m) }
            ),
            (
                "Vệ Sinh Giày Sneakers",
                "Làm sạch chuyên sâu giày sneaker.",
                TypeStatus.SingleService,
                "Vệ Sinh",
                new[] { ("Đôi", true, 1m, 40000m, 30m) }
            ),
        };

        var imageDir = Path.Combine(
            AppContext.BaseDirectory,
            "Resources",
            "SeedImages",
            "Services"
        );
        var serviceEntities = new List<Service>();

        for (int i = 0; i < servicesToSeed.Length; i++)
        {
            var (name, description, type, categoryName, unitsData) = servicesToSeed[i];
            var slug = Generator.GenerateSlug(name);
            var matchingFile = Directory
                .EnumerateFiles(imageDir, "*.png", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(imageDir, "*.jpg", SearchOption.AllDirectories))
                .FirstOrDefault(f =>
                    Path.GetFileNameWithoutExtension(f)
                        .Equals(slug, StringComparison.OrdinalIgnoreCase)
                );

            string? imageUrl = null;
            if (matchingFile != null)
            {
                try
                {
                    var formFile = GenerateIFormFile(matchingFile);
                    var key = media.GetKey(formFile, MediaType.Image);
                    await media.UploadMediaAsync(formFile, key);
                    imageUrl = key;
                }
                catch (Exception ex)
                {
                    logger.Warning(
                        ex,
                        $"Lỗi khi upload ảnh cho dịch vụ '{name}' (slug: {slug}): {ex.Message}"
                    );
                }
            }
            else
            {
                logger.Warning($"Không tìm thấy ảnh cho dịch vụ '{name}' (slug: {slug})");
            }

            if (!categories.ContainsKey(categoryName))
            {
                logger.Warning($"Danh mục '{categoryName}' không tồn tại cho dịch vụ '{name}'.");
                continue;
            }

            var service = new Service(
                categoryId: categories[categoryName],
                branchId: 1L,
                name: name,
                type: type,
                status: ActivationStatus.Active,
                description: description,
                image: imageUrl
            )
            {
                Disable = false,
                Slug = slug,
                CreatedAt = GenerateDistributedDate(i, servicesToSeed.Length),
            };

            foreach (var (unitName, isBaseUnit, multiple, price, processingTime) in unitsData)
            {
                if (!units.ContainsKey(unitName))
                {
                    logger.Warning($"Đơn vị '{unitName}' không tồn tại cho dịch vụ '{name}'.");
                    continue;
                }

                if (isBaseUnit && multiple != 1m)
                {
                    logger.Warning(
                        $"Đơn vị cơ bản '{unitName}' cho dịch vụ '{name}' phải có Multiple = 1."
                    );
                    continue;
                }

                if (!isBaseUnit && multiple <= 0)
                {
                    logger.Warning(
                        $"Đơn vị không cơ bản '{unitName}' cho dịch vụ '{name}' phải có Multiple > 0."
                    );
                    continue;
                }

                service.UnitRelations.Add(
                    new UnitRelation
                    {
                        UnitId = units[unitName].Id,
                        Name = unitName,
                        BaseUnit = isBaseUnit,
                        Price = price,
                        Multiple = (int)multiple,
                        ProcessingTime = processingTime,
                        Status = ActivationStatus.Active,
                        CreatedAt = service.CreatedAt,
                    }
                );
            }

            if (!service.UnitRelations.Any(r => r.BaseUnit))
            {
                logger.Warning($"Dịch vụ '{name}' thiếu đơn vị cơ bản.");
                continue;
            }

            serviceEntities.Add(service);
        }

        await unitOfWork.Repository<Service>().AddRangeAsync(serviceEntities, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
    }

    private static async Task InitializeBranchProductsAsync(
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        var categories = (
            await unitOfWork.Repository<Category>().ListAsync(cancellationToken)
        ).ToDictionary(c => c.Name, c => c.Id);
        var units = (await unitOfWork.Repository<Unit>().ListAsync(cancellationToken)).ToDictionary(
            u => u.Name,
            u => u
        );

        if (!categories.Any() || !units.Any())
        {
            logger.Warning("Thiếu danh mục hoặc đơn vị tính để khởi tạo sản phẩm.");
            return;
        }

        var productsToSeed = new (
            string Name,
            string Category,
            (
                string Unit,
                bool IsBaseUnit,
                decimal Multiple,
                decimal RetailPrice,
                decimal CapitalPrice
            )[] Units
        )[]
        {
            (
                "Bột giặt Omo",
                "Nước giặt",
                new[]
                {
                    ("Hộp", true, 1m, 95000m, 80000m), // Base unit: 1 box
                }
            ),
            (
                "Nước xả Downy",
                "Nước xả",
                new[]
                {
                    ("Chai", true, 1m, 75000m, 60000m), // Base unit: 1 bottle
                    ("Lít", false, 3m, 25000m, 20000m), // 3 liters = 1 bottle
                }
            ),
            (
                "Túi giặt lưới",
                "Combo",
                new[]
                {
                    ("Bộ", true, 1m, 30000m, 20000m), // Base unit: 1 set
                }
            ),
            (
                "Chất tẩy Javel",
                "Nước vệ sinh",
                new[]
                {
                    ("Chai", true, 1m, 35000m, 25000m), // Base unit: 1 bottle
                    ("Lít", false, 1m, 35000m, 25000m), // 1 liter = 1 bottle
                }
            ),
            (
                "Xịt thơm quần áo",
                "Nước xả",
                new[]
                {
                    ("Chai", true, 1m, 50000m, 40000m), // Base unit: 1 bottle
                    ("Lít", false, 0.5m, 100000m, 80000m), // 0.5 liters = 1 bottle
                }
            ),
            (
                "Nước giặt Ariel",
                "Nước giặt",
                new[]
                {
                    ("Chai", true, 1m, 85000m, 70000m), // Base unit: 1 bottle
                    ("Lít", false, 3m, 28333.33m, 23333.33m), // 3 liters = 1 bottle
                }
            ),
            (
                "Viên giặt Tide",
                "Nước giặt",
                new[]
                {
                    ("Hộp", true, 1m, 110000m, 90000m), // Base unit: 1 box
                }
            ),
            (
                "Nước vệ sinh máy",
                "Nước vệ sinh",
                new[]
                {
                    ("Chai", true, 1m, 40000m, 30000m), // Base unit: 1 bottle
                    ("Lít", false, 1m, 40000m, 30000m), // 1 liter = 1 bottle
                }
            ),
            (
                "Chổi lông gà",
                "Vệ Sinh",
                new[]
                {
                    ("Bộ", true, 1m, 25000m, 15000m), // Base unit: 1 set
                }
            ),
            (
                "Găng tay cao su",
                "Vệ Sinh",
                new[]
                {
                    ("Đôi", true, 1m, 30000m, 20000m), // Base unit: 1 pair
                }
            ),
        };

        var products = new List<BranchProduct>();
        for (int i = 0; i < productsToSeed.Length; i++)
        {
            var (name, categoryName, unitsData) = productsToSeed[i];
            if (!categories.ContainsKey(categoryName))
            {
                logger.Warning($"Danh mục '{categoryName}' không tồn tại cho sản phẩm '{name}'.");
                continue;
            }

            var product = new BranchProduct(
                branchId: 1L,
                name: name,
                sku: Generator.GenerateCode("BP", 6),
                status: ActivationStatus.Active,
                capitalPrice: unitsData.First(u => u.IsBaseUnit).CapitalPrice, // Use base unit capital price
                categoryId: categories[categoryName],
                description: $"Sản phẩm dùng trong giặt ủi: {name}",
                image: null
            )
            {
                CreatedAt = GenerateDistributedDate(i, productsToSeed.Length),
            };

            foreach (var (unitName, isBaseUnit, multiple, retailPrice, capitalPrice) in unitsData)
            {
                if (!units.ContainsKey(unitName))
                {
                    logger.Warning($"Đơn vị '{unitName}' không tồn tại cho sản phẩm '{name}'.");
                    continue;
                }

                if (isBaseUnit && multiple != 1m)
                {
                    logger.Warning(
                        $"Đơn vị cơ bản '{unitName}' cho sản phẩm '{name}' phải có Multiple = 1."
                    );
                    continue;
                }

                if (!isBaseUnit && multiple <= 0)
                {
                    logger.Warning(
                        $"Đơn vị không cơ bản '{unitName}' cho sản phẩm '{name}' phải có Multiple > 0."
                    );
                    continue;
                }

                product.UnitRelations.Add(
                    new UnitRelation
                    {
                        UnitId = units[unitName].Id, // Reference Unit.Id from Unit table
                        Name = unitName, // For display purposes (optional if UnitId is sufficient)
                        BaseUnit = isBaseUnit,
                        Price = retailPrice,
                        Multiple = (int)multiple,
                        ProcessingTime = 0,
                        Status = ActivationStatus.Active,
                        CreatedAt = product.CreatedAt,
                    }
                );
            }

            if (!product.UnitRelations.Any(r => r.BaseUnit))
            {
                logger.Warning($"Sản phẩm '{name}' thiếu đơn vị cơ bản.");
                continue;
            }

            products.Add(product);
        }

        await unitOfWork.Repository<BranchProduct>().AddRangeAsync(products, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
    }

    private static async Task InitializeTariffsAsync(
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        var services = await unitOfWork
            .DynamicReadOnlyRepository<Service>()
            .ListAsync(
                new ListServiceSpecification(),
                new QueryParamRequest { },
                s => new
                {
                    s.Id,
                    s.Name,
                    UnitRelations = s
                        .UnitRelations.Where(x =>
                            x.Status.Equals(ActivationStatus.Active) && x.Price > 0
                        )
                        .Select(x => new
                        {
                            x.Id,
                            x.UnitId,
                            x.Name,
                            x.Price,
                            x.ProcessingTime,
                            x.Status,
                        })
                        .ToList(),
                },
                cancellationToken
            );

        if (!services.Any())
        {
            logger.Warning("Không tìm thấy dịch vụ nào trong cơ sở dữ liệu.");
            return;
        }

        var validServices = services.Where(s => s.UnitRelations.Any()).ToList();
        if (!validServices.Any())
        {
            logger.Warning("Không tìm thấy dịch vụ nào có unit_relations hợp lệ.");
            return;
        }

        var branchIds = new long[] { 1, 2, 3 };
        var tariffMonths = new List<(string Name, DateTimeOffset StartAt, DateTimeOffset EndAt)>
        {
            (
                "Bảng Giá Tháng 10/2024",
                new DateTimeOffset(2024, 10, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2024, 10, 31, 23, 59, 59, TimeSpan.Zero)
            ),
            (
                "Bảng Giá Tháng 11/2024",
                new DateTimeOffset(2024, 11, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2024, 11, 30, 23, 59, 59, TimeSpan.Zero)
            ),
            (
                "Bảng Giá Tháng 12/2024",
                new DateTimeOffset(2024, 12, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero)
            ),
            (
                "Bảng Giá Tháng 1/2025",
                new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 1, 31, 23, 59, 59, TimeSpan.Zero)
            ),
            (
                "Bảng Giá Tháng 2/2025",
                new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 2, 28, 23, 59, 59, TimeSpan.Zero)
            ),
            (
                "Bảng Giá Tháng 3/2025",
                new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 3, 31, 23, 59, 59, TimeSpan.Zero)
            ),
            (
                "Bảng Giá Tháng 4/2025",
                new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 4, 30, 23, 59, 59, TimeSpan.Zero)
            ),
            (
                "Bảng Giá Tháng 5/2025",
                new DateTimeOffset(2025, 5, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 5, 31, 23, 59, 59, TimeSpan.Zero)
            ),
            (
                "Bảng Giá Tháng 6/2025",
                new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 6, 30, 23, 59, 59, TimeSpan.Zero)
            ),
            (
                "Bảng Giá Tháng 7/2025",
                new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 7, 31, 23, 59, 59, TimeSpan.Zero)
            ),
            (
                "Bảng Giá Tháng 8/2025",
                new DateTimeOffset(2025, 8, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 8, 6, 23, 59, 59, TimeSpan.Zero)
            ),
        };

        int tariffsCreated = 0;
        int serviceTariffsCreated = 0;

        for (int i = 0; i < tariffMonths.Count; i++)
        {
            var (name, startAt, endAt) = tariffMonths[i];
            var tariff = new Tariff(
                name: name,
                branchId: branchIds[i % branchIds.Length],
                status: startAt <= DateTimeOffset.Now
                    ? ActivationStatus.Active
                    : ActivationStatus.Inactive,
                startAt: startAt,
                endAt: endAt
            )
            {
                CreatedAt = startAt,
            };

            var usedServiceUnitPairs = new HashSet<(long serviceId, long unitRelationId)>();
            var selectedServices = validServices.Take(4).ToList();

            foreach (var service in selectedServices)
            {
                var unitRelation = service.UnitRelations.FirstOrDefault();
                if (unitRelation == null)
                    continue;

                var serviceTariff = new ServiceTariff
                {
                    TariffId = tariff.Id,
                    ServiceId = service.Id,
                    UnitRelationId = unitRelation.Id,
                    Price = unitRelation.Price,
                    Tariff = tariff,
                    Service = null!,
                    UnitRelation = null!,
                    CreatedAt = startAt,
                };

                tariff.ServiceTariffs.Add(serviceTariff);
                usedServiceUnitPairs.Add((service.Id, unitRelation.Id));
                serviceTariffsCreated++;
            }

            if (tariff.ServiceTariffs.Any())
            {
                await unitOfWork.Repository<Tariff>().AddAsync(tariff, cancellationToken);
                tariffsCreated++;
            }
        }

        try
        {
            await unitOfWork.SaveAsync(cancellationToken);
            logger.Information(
                $"Đã khởi tạo {tariffsCreated} bảng giá và {serviceTariffsCreated} dịch vụ trong bảng giá."
            );
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
        {
            logger.Error(
                ex,
                $"Lỗi lưu dữ liệu do vi phạm ràng buộc khóa duy nhất: {pgEx.MessageText}"
            );
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Lỗi không xác định khi lưu bảng giá và dịch vụ trong bảng giá.");
            throw;
        }
    }

    private static async Task InitVouchersAsync(
        IUnitOfWork unitOfWork,
        IQrGenerator barcode,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        var customerResult = await unitOfWork
            .Repository<User>()
            .QueryAsync(x => x.Role == "CUSTOMER")
            .Select(SelectOnlyId())
            .ToListAsync(cancellationToken);

        if (!customerResult.Any())
        {
            logger.Warning("Không tìm thấy khách hàng để khởi tạo voucher.");
            return;
        }

        var vouchersToSeed = new (
            string Code,
            string Title,
            bool DiscountFixed,
            decimal DiscountValue,
            string Description,
            int ValidDays
        )[]
        {
            (
                "GIATUI10",
                "Giảm Giá Chào Mừng",
                false,
                10.0m,
                "Giảm 10% cho khách hàng mới sử dụng dịch vụ giặt ủi",
                30
            ),
            (
                "HAPPYBIRTHDAY",
                "Giảm Giá Sinh Nhật",
                false,
                10.0m,
                "Giảm 10% cho khách hàng trong tháng sinh nhật",
                7
            ),
            (
                "SUMMER15",
                "Ưu Đãi Mùa Hè",
                false,
                15.0m,
                "Giảm 15% cho tất cả dịch vụ giặt ủi trong mùa hè",
                60
            ),
            (
                "FIXED20K",
                "Giảm Giá Cố Định",
                true,
                20000.0m,
                "Giảm 20.000 VNĐ cho hóa đơn giặt ủi tiếp theo",
                90
            ),
        };

        var vouchers = new List<Voucher>();
        for (int i = 0; i < vouchersToSeed.Length; i++)
        {
            var (code, title, discountFixed, discountValue, description, validDays) =
                vouchersToSeed[i];
            var startAt = GenerateDistributedDate(i, vouchersToSeed.Length);
            var voucher = new Voucher(
                code: code,
                title: title,
                imgUrl: null,
                barcode: barcode.GenerateQrBase64(code),
                discountFixed: discountFixed,
                discountValue: discountValue,
                startAt: startAt,
                endAt: startAt.AddDays(validDays),
                status: ActivationStatus.Active,
                description: description
            )
            {
                CreatedAt = startAt,
                VoucherCustomers = customerResult
                    .Select(x => new VoucherCustomer
                    {
                        CustomerId = x.Id,
                        IsUsed = startAt < EndDate.AddMonths(-1),
                    })
                    .ToList(),
            };
            vouchers.Add(voucher);
        }

        await unitOfWork.Repository<Voucher>().AddRangeAsync(vouchers, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
    }

    private static async Task InitializeOrdersAsync(
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken,
        IEncryptionService encryption,
        IQrGenerator barcode
    )
    {
        var customerResult = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .ListAsync(
                new ListUserSpecification([ROLE.CUSTOMER]),
                new QueryParamRequest { },
                SelectOnlyId(),
                cancellationToken
            );
        var staffResult = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .ListAsync(
                new ListUserSpecification([ROLE.STAFF]),
                new QueryParamRequest { },
                SelectOnlyId(),
                cancellationToken
            );
        var tariffs = await unitOfWork
            .Repository<Tariff>()
            .QueryAsync(x => x.Status == ActivationStatus.Active)
            .ToListAsync(cancellationToken);
        var services = await unitOfWork
            .DynamicReadOnlyRepository<Service>()
            .ListAsync(
                new ListServiceSpecification(),
                new QueryParamRequest { },
                s => new
                {
                    s.Id,
                    s.Name,
                    s.Type,
                    UnitRelations = s
                        .UnitRelations.Where(x =>
                            x.Status.Equals(ActivationStatus.Active) && x.Price > 0
                        )
                        .Select(x => new
                        {
                            x.Id,
                            x.UnitId,
                            x.Name,
                            x.Price,
                            x.ProcessingTime,
                            x.Status,
                        })
                        .ToList(),
                },
                cancellationToken
            );

        if (!customerResult.Any() || !staffResult.Any() || !tariffs.Any() || !services.Any())
        {
            logger.Warning(
                "Thiếu dữ liệu khách hàng, nhân viên, bảng giá hoặc dịch vụ để khởi tạo đơn hàng."
            );
            return;
        }

        const int totalOrders = 50;
        var statusValues = new[]
        {
            OrderStatus.Pending,
            OrderStatus.InProgress,
            OrderStatus.Processed,
            OrderStatus.Completed,
        };

        for (int i = 0; i < totalOrders; i++)
        {
            var createdAt = GenerateDistributedDate(i, totalOrders);
            long? customerId = customerResult[i % customerResult.Count].Id;
            long staffId = staffResult[i % staffResult.Count].Id;
            var tariff = tariffs[i % tariffs.Count];
            int itemCount = services.Any(s => s.Type == TypeStatus.Combo) ? 2 : 1;
            var orderItems = new List<OrderItem>();
            decimal totalPrice = 0;

            for (int j = 0; j < itemCount; j++)
            {
                var service = services[j % services.Count];
                var unitRelations = service.UnitRelations.ToList();
                if (!unitRelations.Any())
                {
                    logger.Warning($"Không tìm thấy unit_relation cho dịch vụ ID {service.Id}.");
                    continue;
                }

                var unitRelation = unitRelations.First();
                int quantity = service.Type == TypeStatus.Combo ? 3 : 1;
                decimal unitPrice = unitRelation.Price;

                var orderItem = new OrderItem
                {
                    ServiceId = service.Id,
                    UnitRelationId = unitRelation.Id,
                    Price = quantity * unitPrice,
                    Quantity = quantity,
                    CreatedAt = createdAt,
                    UnitRelationName = unitRelation.Name,
                    ProcessingTime = (int)unitRelation.ProcessingTime,
                    ServiceName = service.Name,
                    UnitPrice = unitPrice,
                };

                orderItems.Add(orderItem);
                totalPrice += orderItem.Price;
            }

            if (!orderItems.Any())
            {
                logger.Warning($"Không tạo được OrderItem cho đơn hàng {i + 1}.");
                continue;
            }

            string code = Generator.GenerateCode("OD", 6);
            OrderStatus status = statusValues[i % statusValues.Length];

            var order = new Order(
                branchId: tariff.BranchId,
                staffId: staffId,
                code: code,
                amount: totalPrice,
                total: totalPrice,
                status: status,
                customerId: customerId,
                tariffId: tariff.Id,
                deliveryTime: createdAt.AddDays(3)
            )
            {
                CreatedAt = createdAt,
            };

            var codeEncrypt = encryption.Encrypt(order.Code);
            var barcodeConfirm = barcode.GenerateQrBase64(codeEncrypt);
            order.CodeConfirm = barcodeConfirm;
            order.PublicId = Ulid.NewUlid();

            foreach (var orderItem in orderItems)
            {
                order.OrderItems.Add(orderItem);
            }

            if (status == OrderStatus.Completed)
            {
                var paymentMethods = Enum.GetValues(typeof(PaymentMethod))
                    .Cast<PaymentMethod>()
                    .ToArray();
                order.PaymentMethod = paymentMethods[i % paymentMethods.Length];
            }

            await unitOfWork.Repository<Order>().AddAsync(order, cancellationToken);

            await unitOfWork.SaveAsync(cancellationToken);
            Order? orderUpdate = await unitOfWork
                .DynamicReadOnlyRepository<Order>()
                .FindByConditionAsync(new GetOrderByIdSpecification(order.Id), cancellationToken);
            if (orderUpdate != null)
            {
                orderUpdate.UpdateStatus(status);
                await unitOfWork.Repository<Order>().UpdateAsync(orderUpdate);

                await unitOfWork.SaveAsync(cancellationToken);
            }
        }
    }

    private static async Task InitializeInventoryDocumentsAsync(
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        var suppliers = await unitOfWork.Repository<Supplier>().ListAsync(cancellationToken);
        var products = await unitOfWork.Repository<BranchProduct>().ListAsync(cancellationToken);
        var units = await unitOfWork.Repository<Unit>().ListAsync(cancellationToken);

        if (!suppliers.Any() || !products.Any() || !units.Any())
        {
            logger.Warning("Thiếu nhà cung cấp, sản phẩm hoặc đơn vị tính.");
            return;
        }

        var createdAt = GenerateDistributedDate(0, 1);
        var supplier = suppliers.First();
        decimal totalProductAmount = 0;
        var productSupplyings = new List<ProductSupplying>();

        foreach (var product in products)
        {
            var unitRelation = product.UnitRelations.FirstOrDefault(r => r.BaseUnit);
            if (unitRelation == null)
            {
                logger.Warning($"Sản phẩm {product.Name} chưa có đơn vị cơ bản.");
                continue;
            }

            int quantity = product.Name.Contains("giặt", StringComparison.OrdinalIgnoreCase)
                ? 20
                : 10;
            decimal amount = unitRelation.Price * quantity;
            totalProductAmount += amount;

            productSupplyings.Add(
                new ProductSupplying
                {
                    ProductId = product.Id,
                    SupplierId = supplier.Id,
                    Quantity = quantity,
                    LotNumber = Generator.GenerateCode("LOT", 4),
                    Price = unitRelation.Price,
                    UnitRelationId = unitRelation.Id,
                    ExpiryDate = createdAt.AddMonths(12),
                    CreatedAt = createdAt,
                }
            );
        }

        decimal totalEquipmentAmount = 0;
        var equipmentSupplyings = new List<EquipmentSupplying>
        {
            new EquipmentSupplying
            {
                Name = "Máy Giặt Công Nghiệp",
                Code = Generator.GenerateCode("EQ", 6),
                Price = 20000000,
                Quantity = 2,
                SupplierId = supplier.Id,
                CreatedAt = createdAt,
            },
            new EquipmentSupplying
            {
                Name = "Máy Sấy Công Nghiệp",
                Code = Generator.GenerateCode("EQ", 6),
                Price = 15000000,
                Quantity = 1,
                SupplierId = supplier.Id,
                CreatedAt = createdAt,
            },
        };
        totalEquipmentAmount = equipmentSupplyings.Sum(e => e.Price * e.Quantity);

        var document = new InventoryDocument(
            code: Generator.GenerateCode("IM", 6),
            amount: totalProductAmount + totalEquipmentAmount,
            type: InventoryType.Import,
            branchId: 1L,
            note: "Nhập hàng khởi tạo"
        )
        {
            CreatedAt = createdAt,
        };

        foreach (var p in productSupplyings)
        {
            document.ProductSupplyings.Add(p);
        }

        foreach (var e in equipmentSupplyings)
        {
            document.EquipmentSupplyings.Add(e);
        }

        await unitOfWork.Repository<InventoryDocument>().AddAsync(document, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        document.UpdateStatus(InventoryStatus.Completed);
        await unitOfWork.SaveAsync(cancellationToken);
    }

    private static IFormFile GenerateIFormFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Không tìm thấy file tại {filePath}");

        byte[] fileBytes = File.ReadAllBytes(filePath);
        var memoryStream = new MemoryStream(fileBytes);
        var fileName = Path.GetFileName(filePath);
        var formFile = new FormFile(memoryStream, 0, fileBytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = GetContentType(filePath),
        };
        return formFile;
    }

    private static string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
    }
}
