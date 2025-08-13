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
using Application.Feature.Common.Projections.Inventories;
using Application.Feature.InventoryDocuments.Commands.Create;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Contracts.Dtos.Models;
using Contracts.Dtos.Requests;
using Contracts.Utils;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using Domain.Aggregates.Suppliers;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Domain.Aggregates.Vouchers;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Specification;
using Wangkanai.Extensions;

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
        12,
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
            if (
                !await unitOfWork
                    .Repository<Domain.Aggregates.Services.Unit>()
                    .AnyAsync(cancellationToken: cancellationToken)
            )
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
                    .Repository<Voucher>()
                    .AnyAsync(cancellationToken: cancellationToken)
            )
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu voucher...");
                await InitVouchersAsync(unitOfWork, qrGenerator, logger, cancellationToken);
                logger.Information("Hoàn tất khởi tạo dữ liệu voucher.");
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
        var units = new List<string> { "Kg", "Bộ", "Mét", "Lít", "Đôi", "Hộp", "Thùng", "Chai" };

        var random = new Random();

        for (int i = 0; i < units.Count; i++)
        {
            var name = units[i];
            var unit = new Domain.Aggregates.Services.Unit(
                name: name,
                status: ActivationStatus.Active
            )
            {
                CreatedAt = new DateTimeOffset(
                    year: 2024,
                    month: 12,
                    day: random.Next(1, 28),
                    hour: random.Next(0, 24),
                    minute: random.Next(0, 60),
                    second: random.Next(0, 60),
                    offset: TimeSpan.FromHours(0)
                ),
            };

            await unitOfWork
                .Repository<Domain.Aggregates.Services.Unit>()
                .AddAsync(unit, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
        }
    }

    private static async Task InitializeCategoriesAsync(
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken
    )
    {
        var categoryNames = new List<string>
        {
            "Giặt",
            "Ủi",
            "Sấy",
            "Vệ Sinh",
            "Combo",
            "Giặt Đặc Biệt",
            "Nước giặt",
            "Nước xả",
            "Nước vệ sinh",
        };

        var random = new Random();

        for (int i = 0; i < categoryNames.Count; i++)
        {
            var id = i + 1; // Id bắt đầu từ 1
            var code = $"DM{id:D6}"; // DM000001, DM000002, ...

            var category = new Category(
                name: categoryNames[i],
                parentId: null,
                status: ActivationStatus.Active,
                code: code
            )
            {
                Id = id, // Gán Id thủ công (nếu entity cho phép set)
                Disabled = false,
                Path = code.ToLowerInvariant(),
                CreatedAt = new DateTimeOffset(
                    year: 2024,
                    month: 12,
                    day: random.Next(1, 28),
                    hour: random.Next(0, 24),
                    minute: random.Next(0, 60),
                    second: random.Next(0, 60),
                    offset: TimeSpan.FromHours(0)
                ),
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
        var random = new Random();
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
                CreatedAt = new DateTimeOffset(
                    year: 2024,
                    month: 12,
                    day: random.Next(1, 28),
                    hour: random.Next(0, 24),
                    minute: random.Next(0, 60),
                    second: random.Next(0, 60),
                    offset: TimeSpan.FromHours(0)
                ),
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
                CreatedAt = new DateTimeOffset(
                    year: 2024,
                    month: 12,
                    day: random.Next(1, 28),
                    hour: random.Next(0, 24),
                    minute: random.Next(0, 60),
                    second: random.Next(0, 60),
                    offset: TimeSpan.FromHours(0)
                ),
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
                CreatedAt = new DateTimeOffset(
                    year: 2024,
                    month: 12,
                    day: random.Next(1, 28),
                    hour: random.Next(0, 24),
                    minute: random.Next(0, 60),
                    second: random.Next(0, 60),
                    offset: TimeSpan.FromHours(0)
                ),
            },
        };

        await unitOfWork.Repository<Supplier>().AddRangeAsync(suppliers, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
    }

    private static async Task InitializeServicesAsync(
        IUnitOfWork unitOfWork,
        ILogger logger,
        IMediaUpdateService media,
        CancellationToken cancellationToken
    )
    {
        // Load dữ liệu category, user admin, unit, product (branch product)
        var categories = (
            await unitOfWork.Repository<Category>().ListAsync(cancellationToken)
        ).ToDictionary(c => c.Name, c => c.Id);
        var user = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(
                new ListUserSpecification(new[] { ROLE.ADMIN }),
                cancellationToken: cancellationToken
            );
        var units = (
            await unitOfWork
                .Repository<Domain.Aggregates.Services.Unit>()
                .ListAsync(cancellationToken)
        ).ToDictionary(u => u.Name, u => u);
        var products = (
            await unitOfWork.Repository<BranchProduct>().ListAsync(cancellationToken)
        ).ToDictionary(p => p.Name, p => p);

        if (user == null)
            throw new InvalidOperationException("Admin user not found.");
        if (!categories.Any() || !units.Any() || !products.Any())
            throw new InvalidOperationException("Missing Category, Unit or Product data.");

        // Dữ liệu seed dịch vụ kèm đơn vị và service resource (liên kết sản phẩm + qty)
        var servicesToSeed = new[]
        {
            (
                Name: "Combo Giặt Sấy Quần Áo",
                Description: "Dịch vụ combo giặt và sấy quần áo tiện lợi.",
                CategoryName: "Combo",
                Units: new[]
                {
                    (
                        UnitName: "Kg",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 8000m,
                        ProcessingTime: 20m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Hộp", ProductName: "Bột giặt Omo", Quantity: 0.1m),
                    (UnitName: "Lít", ProductName: "Nước xả Downy", Quantity: 0.05m),
                }
            ),
            (
                Name: "Combo Giặt Sấy Ủi Quần Áo",
                Description: "Combo toàn diện: giặt, sấy và ủi quần áo.",
                CategoryName: "Combo",
                Units: new[]
                {
                    (
                        UnitName: "Kg",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 10000m,
                        ProcessingTime: 30m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Hộp", ProductName: "Bột giặt Omo", Quantity: 0.1m),
                    (UnitName: "Lít", ProductName: "Nước xả Downy", Quantity: 0.05m),
                }
            ),
            (
                Name: "Combo Giặt Sấy Ủi Tuần Tháng",
                Description: "Gói combo cho giặt sấy ủi đồ định kỳ tuần/tháng.",
                CategoryName: "Combo",
                Units: new[]
                {
                    (
                        UnitName: "Kg",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 8000m,
                        ProcessingTime: 20m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Hộp", ProductName: "Bột giặt Omo", Quantity: 0.1m),
                    (UnitName: "Lít", ProductName: "Nước xả Downy", Quantity: 0.05m),
                }
            ),
            (
                Name: "Giặt Chăn Mền Dày",
                Description: "Làm sạch sâu chăn mền dày và nặng.",
                CategoryName: "Giặt",
                Units: new[]
                {
                    (
                        UnitName: "Bộ",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 15000m,
                        ProcessingTime: 60m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Hộp", ProductName: "Bột giặt Omo", Quantity: 0.2m),
                    (UnitName: "Lít", ProductName: "Nước xả Downy", Quantity: 0.1m),
                }
            ),
            (
                Name: "Giặt Đồ Trẻ Em",
                Description: "Giặt đồ nhẹ nhàng an toàn cho trẻ nhỏ.",
                CategoryName: "Giặt",
                Units: new[]
                {
                    (
                        UnitName: "Kg",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 6000m,
                        ProcessingTime: 20m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Hộp", ProductName: "Bột giặt Omo", Quantity: 0.08m),
                    (UnitName: "Lít", ProductName: "Nước xả Downy", Quantity: 0.04m),
                }
            ),
            (
                Name: "Giặt Hấp Váy Dạ Hội",
                Description: "Giặt hấp cao cấp cho váy dạ hội và đồ cao cấp.",
                CategoryName: "Giặt Đặc Biệt",
                Units: new[]
                {
                    (
                        UnitName: "Bộ",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 12000m,
                        ProcessingTime: 90m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Lít", ProductName: "Chất tẩy Javel", Quantity: 0.05m),
                }
            ),
            (
                Name: "Giặt Khô Áo Vest",
                Description: "Giặt khô chuyên dụng cho áo vest.",
                CategoryName: "Giặt Đặc Biệt",
                Units: new[]
                {
                    (
                        UnitName: "Bộ",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 12000m,
                        ProcessingTime: 60m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Lít", ProductName: "Chất tẩy Javel", Quantity: 0.05m),
                }
            ),
            (
                Name: "Giặt Quần Áo Trắng",
                Description: "Tẩy trắng, làm sạch sâu cho quần áo trắng.",
                CategoryName: "Giặt",
                Units: new[]
                {
                    (
                        UnitName: "Kg",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 10000m,
                        ProcessingTime: 20m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Hộp", ProductName: "Bột giặt Omo", Quantity: 0.1m),
                    (UnitName: "Lít", ProductName: "Chất tẩy Javel", Quantity: 0.02m),
                }
            ),
            (
                Name: "Giặt Tẩy Vết Bẩn Cứng Đầu",
                Description: "Tẩy vết bẩn khó xử lý trên quần áo.",
                CategoryName: "Giặt Đặc Biệt",
                Units: new[]
                {
                    (
                        UnitName: "Bộ",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 10000m,
                        ProcessingTime: 30m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Lít", ProductName: "Chất tẩy Javel", Quantity: 0.05m),
                }
            ),
            (
                Name: "Giặt Thảm",
                Description: "Làm sạch thảm trải sàn tại nhà hoặc văn phòng.",
                CategoryName: "Giặt",
                Units: new[]
                {
                    (
                        UnitName: "Mét",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 15000m,
                        ProcessingTime: 30m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Hộp", ProductName: "Bột giặt Omo", Quantity: 0.15m),
                    (UnitName: "Lít", ProductName: "Chất tẩy Javel", Quantity: 0.03m),
                }
            ),
            (
                Name: "Sấy Khô Chăn",
                Description: "Sấy khô chăn mền nhanh chóng và an toàn.",
                CategoryName: "Sấy",
                Units: new[]
                {
                    (
                        UnitName: "Bộ",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 5000m,
                        ProcessingTime: 30m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Lít", ProductName: "Nước xả Downy", Quantity: 0.1m),
                }
            ),
            (
                Name: "Sấy Khô Quần Áo",
                Description: "Sấy khô quần áo chống ẩm mốc.",
                CategoryName: "Sấy",
                Units: new[]
                {
                    (
                        UnitName: "Kg",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 5000m,
                        ProcessingTime: 15m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Lít", ProductName: "Nước xả Downy", Quantity: 0.05m),
                }
            ),
            (
                Name: "Ủi Áo Sơ Mi",
                Description: "Ủi áo sơ mi chuyên nghiệp, gọn gàng.",
                CategoryName: "Ủi",
                Units: new[]
                {
                    (
                        UnitName: "Bộ",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 5000m,
                        ProcessingTime: 15m
                    ),
                },
                ServiceResources: new (string UnitName, string ProductName, decimal Quantity)[] { } // Không cần vật tư tiêu hao
            ),
            (
                Name: "Vệ Sinh Giày Sneakers",
                Description: "Làm sạch chuyên sâu giày sneaker.",
                CategoryName: "Vệ Sinh",
                Units: new[]
                {
                    (
                        UnitName: "Đôi",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        Price: 40000m,
                        ProcessingTime: 30m
                    ),
                },
                ServiceResources: new[]
                {
                    (UnitName: "Lít", ProductName: "Chất tẩy Javel", Quantity: 0.02m),
                }
            ),
        };

        var imageDir = Path.Combine(
            AppContext.BaseDirectory,
            "Resources",
            "SeedImages",
            "Services"
        );
        var serviceEntities = new List<Service>();
        var random = new Random();

        foreach (var svc in servicesToSeed)
        {
            if (!categories.TryGetValue(svc.CategoryName, out var categoryId))
            {
                logger.Warning(
                    $"Danh mục '{svc.CategoryName}' không tồn tại cho dịch vụ '{svc.Name}'."
                );
                continue;
            }

            var slug = Generator.GenerateSlug(svc.Name);
            string? imageUrl = null;
            if (Directory.Exists(imageDir))
            {
                var matchingFile = Directory
                    .EnumerateFiles(imageDir, "*.png", SearchOption.AllDirectories)
                    .Concat(
                        Directory.EnumerateFiles(imageDir, "*.jpg", SearchOption.AllDirectories)
                    )
                    .FirstOrDefault(f =>
                        Path.GetFileNameWithoutExtension(f)
                            .Equals(slug, StringComparison.OrdinalIgnoreCase)
                    );
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
                            $"Lỗi upload ảnh cho dịch vụ '{svc.Name}': {ex.Message}"
                        );
                    }
                }
                else
                {
                    logger.Warning($"Không tìm thấy ảnh cho dịch vụ '{svc.Name}'.");
                }
            }
            else
            {
                logger.Warning($"Thư mục ảnh '{imageDir}' không tồn tại.");
            }

            var service = new Service(
                categoryId: categoryId,
                branchId: 4,
                name: svc.Name,
                status: ActivationStatus.Active,
                description: svc.Description,
                image: imageUrl
            )
            {
                Disable = false,
                Slug = slug,
                CreatedAt = new DateTimeOffset(
                    2024,
                    12,
                    random.Next(1, 28),
                    random.Next(0, 24),
                    random.Next(0, 60),
                    random.Next(0, 60),
                    TimeSpan.Zero
                ),
            };

            // Tạo UnitRelations + ServiceResources
            foreach (var (unitName, isBaseUnit, multiple, price, processingTime) in svc.Units)
            {
                if (!units.TryGetValue(unitName, out var unitEntity))
                {
                    logger.Warning($"Đơn vị '{unitName}' không tồn tại cho dịch vụ '{svc.Name}'.");
                    continue;
                }

                if (isBaseUnit && multiple != 1m)
                {
                    logger.Warning(
                        $"Đơn vị cơ bản '{unitName}' cho dịch vụ '{svc.Name}' phải có Multiple = 1."
                    );
                    continue;
                }

                if (!isBaseUnit && multiple <= 0)
                {
                    logger.Warning(
                        $"Đơn vị không cơ bản '{unitName}' cho dịch vụ '{svc.Name}' phải có Multiple > 0."
                    );
                    continue;
                }

                var unitRelation = new UnitRelation
                {
                    UnitId = isBaseUnit ? unitEntity.Id : null, // Chỉ gán UnitId cho đơn vị cơ bản
                    Name = unitName,
                    BaseUnit = isBaseUnit,
                    Price = price,
                    Multiple = (int)multiple,
                    ProcessingTime = processingTime,
                    Status = ActivationStatus.Active,
                    CreatedAt = service.CreatedAt,
                };
                // Thêm ServiceResource liên quan cho đơn vị này
                foreach (var (resUnitName, productName, quantity) in svc.ServiceResources)
                {
                    if (!products.TryGetValue(productName, out var branchProduct))
                    {
                        logger.Warning(
                            $"Sản phẩm '{productName}' không tồn tại để thêm ServiceResource cho dịch vụ '{svc.Name}'."
                        );
                        continue;
                    }
                    var unitRelationEntity = branchProduct.UnitRelations.FirstOrDefault(x =>
                        resUnitName.Equals(x.Name, StringComparison.OrdinalIgnoreCase)
                    );

                    if (unitRelationEntity == null)
                    {
                        logger.Warning(
                            $"Không tìm thấy đơn vị '{resUnitName}' cho sản phẩm '{productName}' trong dịch vụ '{svc.Name}'."
                        );
                        continue;
                    }

                    var serviceResource = new ServiceResource
                    {
                        ProductId = branchProduct.Id,
                        UnitProductId = unitRelationEntity.Id,
                        UnitRelationId = unitRelation.Id,
                        Quantity = quantity,
                        CreatedAt = service.CreatedAt,
                    };

                    unitRelation.AsUnitProduct.Add(serviceResource);
                }

                service.UnitRelations.Add(unitRelation);
            }

            if (!service.UnitRelations.Any(r => r.BaseUnit))
            {
                logger.Warning($"Dịch vụ '{svc.Name}' thiếu đơn vị cơ bản.");
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
        var units = (
            await unitOfWork
                .Repository<Domain.Aggregates.Services.Unit>()
                .ListAsync(cancellationToken)
        ).ToDictionary(u => u.Name, u => u);

        if (!units.Any())
        {
            logger.Warning("Không có đơn vị tính để khởi tạo vật tư.");
            return;
        }

        // Giả sử categoryId bạn có sẵn như sau (bạn chỉnh lại đúng id DB thật)
        const long categoryNuocGiatId = 7L;
        const long categoryNuocXaId = 8L;
        const long categoryNuocVeSinhId = 9L;
        const long categoryVeSinhId = 4L;

        var laundryMaterials = new[]
        {
            (
                Name: "Bột giặt Omo",
                CategoryId: categoryNuocGiatId,
                Units: new[]
                {
                    (
                        UnitName: "Hộp",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        RetailPrice: 95000m,
                        CapitalPrice: 80000m
                    ),
                }
            ),
            (
                Name: "Nước xả Downy",
                CategoryId: categoryNuocXaId,
                Units: new[]
                {
                    (
                        UnitName: "Chai",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        RetailPrice: 75000m,
                        CapitalPrice: 60000m
                    ),
                    (
                        UnitName: "Lít",
                        IsBaseUnit: false,
                        Multiple: 3m,
                        RetailPrice: 25000m,
                        CapitalPrice: 20000m
                    ),
                }
            ),
            (
                Name: "Chất tẩy Javel",
                CategoryId: categoryNuocVeSinhId,
                Units: new[]
                {
                    (
                        UnitName: "Chai",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        RetailPrice: 35000m,
                        CapitalPrice: 25000m
                    ),
                    (
                        UnitName: "Lít",
                        IsBaseUnit: false,
                        Multiple: 1m,
                        RetailPrice: 35000m,
                        CapitalPrice: 25000m
                    ),
                }
            ),
            (
                Name: "Găng tay cao su",
                CategoryId: categoryVeSinhId,
                Units: new[]
                {
                    (
                        UnitName: "Đôi",
                        IsBaseUnit: true,
                        Multiple: 1m,
                        RetailPrice: 30000m,
                        CapitalPrice: 20000m
                    ),
                }
            ),
        };

        var products = new List<BranchProduct>();
        var random = new Random();

        for (int i = 0; i < laundryMaterials.Length; i++)
        {
            var item = laundryMaterials[i];

            var product = new BranchProduct(
                branchId: 4,
                name: item.Name,
                sku: Generator.GenerateCode("BP", 6),
                status: ActivationStatus.Active,
                capitalPrice: item.Units.First(u => u.IsBaseUnit).CapitalPrice,
                categoryId: item.CategoryId,
                description: $"Vật tư cửa hàng giặt ủi: {item.Name}",
                image: null
            )
            {
                CreatedAt = new DateTimeOffset(
                    year: 2024,
                    month: 8,
                    day: random.Next(1, 28),
                    hour: random.Next(0, 24),
                    minute: random.Next(0, 60),
                    second: random.Next(0, 60),
                    offset: TimeSpan.FromHours(7)
                ),
            };

            foreach (var unitInfo in item.Units)
            {
                if (!units.ContainsKey(unitInfo.UnitName) && unitInfo.IsBaseUnit)
                {
                    logger.Warning(
                        $"Đơn vị '{unitInfo.UnitName}' không tồn tại cho vật tư '{item.Name}'."
                    );
                    continue;
                }

                if (unitInfo.IsBaseUnit && unitInfo.Multiple != 1m)
                {
                    logger.Warning(
                        $"Đơn vị cơ bản '{unitInfo.UnitName}' của '{item.Name}' phải có Multiple = 1."
                    );
                    continue;
                }

                if (!unitInfo.IsBaseUnit && unitInfo.Multiple <= 0)
                {
                    logger.Warning(
                        $"Đơn vị không cơ bản '{unitInfo.UnitName}' của '{item.Name}' phải có Multiple > 0."
                    );
                    continue;
                }

                product.UnitRelations.Add(
                    new UnitRelation
                    {
                        UnitId = unitInfo.IsBaseUnit ? units[unitInfo.UnitName].Id : (long?)null,
                        Name = unitInfo.UnitName,
                        BaseUnit = unitInfo.IsBaseUnit,
                        Price = unitInfo.RetailPrice,
                        Multiple = (int)unitInfo.Multiple,
                        ProcessingTime = 0,
                        Status = ActivationStatus.Active,
                        CreatedAt = product.CreatedAt,
                    }
                );
            }

            if (!product.UnitRelations.Any(r => r.BaseUnit))
            {
                logger.Warning($"Vật tư '{item.Name}' thiếu đơn vị cơ bản.");
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
        // Check for cancellation at the start
        cancellationToken.ThrowIfCancellationRequested();

        // Fetch services with active unit relations
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
            logger.Warning("No services found in the database.");
            return;
        }

        var validServices = services.Where(s => s.UnitRelations.Any()).ToList();
        if (!validServices.Any())
        {
            logger.Warning("No services with valid unit relations found.");
            return;
        }

        var tariffMonths = new List<(string Name, DateTimeOffset StartAt, DateTimeOffset EndAt)>
        {
            (
                "Bảng giá chung",
                new DateTimeOffset(2024, 10, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 12, 31, 23, 59, 59, TimeSpan.Zero)
            ),
            (
                "Bảng giá tháng 8",
                new DateTimeOffset(2025, 8, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 8, 31, 23, 59, 59, TimeSpan.Zero)
            ),
        };

        int tariffsCreated = 0;
        int serviceTariffsCreated = 0;

        foreach (var (name, startAt, endAt) in tariffMonths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var tariff = new Tariff(
                    name: name,
                    branchId: 4, // Cycle through branch IDs
                    status: startAt <= DateTimeOffset.Now
                        ? ActivationStatus.Active
                        : ActivationStatus.Inactive,
                    startAt: startAt,
                    endAt: endAt
                )
                {
                    CreatedAt = DateTimeOffset.Now, // Set actual creation time
                };

                bool isAugustTariff = name == "Bảng giá tháng 8";
                decimal discount = isAugustTariff ? 0.8m : 1.0m; // Configurable discount

                foreach (var service in validServices)
                {
                    // Select the most appropriate unit relation (e.g., highest priority or first active)
                    var unitRelations = service
                        .UnitRelations.OrderBy(ur => ur.ProcessingTime) // Example: prioritize by processing time
                        .ToList();

                    if (unitRelations == null)
                    {
                        logger.Warning(
                            "No valid unit relation for service {ServiceId}. Skipping.",
                            service.Id
                        );
                        continue;
                    }
                    foreach (var unitRelation in unitRelations)
                    {
                        var serviceTariff = new ServiceTariff
                        {
                            TariffId = tariff.Id,
                            ServiceId = service.Id,
                            UnitRelationId = unitRelation.Id,
                            Price = unitRelation.Price * discount,
                            Tariff = tariff,
                            CreatedAt = DateTimeOffset.Now,
                        };

                        tariff.ServiceTariffs.Add(serviceTariff);
                    }

                    serviceTariffsCreated++;
                }

                if (tariff.ServiceTariffs.Any())
                {
                    await unitOfWork.Repository<Tariff>().AddAsync(tariff, cancellationToken);
                    tariffsCreated++;
                }
                else
                {
                    logger.Warning(
                        "No service tariffs created for tariff {TariffName}. Skipping.",
                        name
                    );
                }

                await unitOfWork.SaveAsync(cancellationToken);

                logger.Information(
                    "Created tariff {TariffName} for branch {BranchId} with {ServiceTariffCount} service tariffs.",
                    name,
                    tariff.BranchId,
                    tariff.ServiceTariffs.Count
                );
            }
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                if (pgEx.SqlState == "23505")
                {
                    logger.Error(
                        ex,
                        "Unique constraint violation while saving tariff {TariffName}: {ErrorMessage}",
                        name,
                        pgEx.MessageText
                    );
                }
                else if (pgEx.SqlState == "23503")
                {
                    logger.Error(
                        ex,
                        "Foreign key violation while saving tariff {TariffName}: {ErrorMessage}",
                        name,
                        pgEx.MessageText
                    );
                }
                else
                {
                    logger.Error(
                        ex,
                        "Database error while saving tariff {TariffName}: {ErrorMessage}",
                        name,
                        pgEx.MessageText
                    );
                }
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unexpected error while saving tariff {TariffName}.", name);
                throw;
            }
        }

        logger.Information(
            "Initialized {TariffCount} tariffs and {ServiceTariffCount} service tariffs.",
            tariffsCreated,
            serviceTariffsCreated
        );
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
        cancellationToken.ThrowIfCancellationRequested();

        var customerResult = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .ListAsync(
                new ListUserSpecification([ROLE.CUSTOMER]),
                new QueryParamRequest { },
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
        var tariffChung = await unitOfWork
            .Repository<Tariff>()
            .QueryAsync(x => x.Status == ActivationStatus.Active && x.Name == "Bảng giá chung")
            .FirstOrDefaultAsync(cancellationToken);
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
                            x.BaseUnit,
                            x.Multiple,
                            AsUnitProduct = x
                                .AsUnitProduct.Select(sr => new
                                {
                                    sr.Quantity,
                                    BranchProductId = sr.BranchProduct.Id,
                                    UnitProductId = sr.UnitProductId,
                                    UnitProductPrice = sr.UnitProduct.Price, // Price from UnitRelation (UnitProduct)
                                })
                                .ToList(),
                        })
                        .ToList(),
                },
                cancellationToken
            );
        var equipments = await unitOfWork
            .Repository<Equipment>()
            .QueryAsync(e => e.Status == EquipmentStatus.Active)
            .ToListAsync(cancellationToken);
        var vouchers = await unitOfWork
            .Repository<Voucher>()
            .QueryAsync(v => v.Status == ActivationStatus.Active)
            .ToListAsync(cancellationToken);

        if (
            !customerResult.Any()
            || !staffResult.Any()
            || tariffChung == null
            || !services.Any()
            || !equipments.Any()
        )
        {
            logger.Warning(
                "Missing data for customers, staff, common tariff, services, or equipment to initialize orders."
            );
            return;
        }

        var random = new Random();
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 8, 13);
        var currentDate = startDate;
        var orders = new List<Order>();
        int orderIndex = 0;
        var paymentMethods = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>().ToArray();
        const int vatPercent = 10;

        while (currentDate <= endDate)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int ordersToday;
            if (currentDate.Month == 1)
            {
                ordersToday = orderIndex < 100 ? (orderIndex < 97 ? 3 : 4) : 0;
            }
            else
            {
                ordersToday = random.Next(10, 51);
            }

            for (int d = 0; d < ordersToday && (currentDate.Month != 1 || orderIndex < 100); d++)
            {
                var createdAt = new DateTimeOffset(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    random.Next(0, 24),
                    random.Next(0, 60),
                    random.Next(0, 60),
                    TimeSpan.FromHours(7)
                );

                var customer = customerResult[orderIndex % customerResult.Count];
                long staffId = staffResult[orderIndex % staffResult.Count].Id;
                int itemCount = random.Next(1, 6);
                var orderItems = new List<OrderItem>();
                decimal amount = 0;

                for (int j = 0; j < itemCount; j++)
                {
                    var serviceIndex = random.Next(0, services.Count);
                    var service = services[serviceIndex];
                    var unitRelations = service.UnitRelations.ToList();
                    if (!unitRelations.Any())
                    {
                        logger.Warning($"No unit_relation found for service ID {service.Id}.");
                        continue;
                    }

                    var unitRelationIndex = random.Next(0, unitRelations.Count);
                    var unitRelation = unitRelations[unitRelationIndex];
                    int quantity = random.Next(1, 6);
                    decimal unitPrice = unitRelation.Price;

                    var orderItem = new OrderItem
                    {
                        ServiceId = service.Id,
                        UnitRelationId = unitRelation.Id,
                        Price = unitPrice,
                        Quantity = quantity,
                        CreatedAt = createdAt,
                        UnitRelationName = unitRelation.Name,
                        ProcessingTime = (int)unitRelation.ProcessingTime,
                        ServiceName = service.Name,
                        UnitPrice = unitPrice,
                    };

                    orderItems.Add(orderItem);
                    amount += unitPrice * quantity;
                }

                if (!orderItems.Any())
                {
                    logger.Warning($"No OrderItem created for order {orderIndex + 1}.");
                    continue;
                }

                bool applyVoucher = random.Next(0, 10) < 3;
                Voucher? selectedVoucher = null;
                if (applyVoucher)
                {
                    var applicableVouchers = vouchers
                        .Where(v => createdAt >= v.StartAt && createdAt <= v.EndAt)
                        .ToList();
                    if (applicableVouchers.Any())
                    {
                        selectedVoucher = applicableVouchers[
                            random.Next(0, applicableVouchers.Count)
                        ];
                    }
                }

                bool discountFixed = selectedVoucher?.DiscountFixed ?? false;
                decimal discountValue = selectedVoucher?.DiscountValue ?? 0m;
                decimal point = random.Next(0, 11) * 50;

                decimal tempTotal = amount;
                if (point > 0)
                {
                    tempTotal -= point * 10;
                }
                if (!discountFixed)
                {
                    tempTotal -= (tempTotal * discountValue / 100);
                }
                else
                {
                    tempTotal -= discountValue;
                }
                decimal vatAmount = tempTotal * vatPercent / 100;
                decimal total = tempTotal + vatAmount;

                var orderEquipments = new List<OrderEquipment>();
                int eqCount = random.Next(1, 3);
                for (int k = 0; k < eqCount; k++)
                {
                    var eqIndex = random.Next(0, equipments.Count);
                    var equipment = equipments[eqIndex];
                    orderEquipments.Add(
                        new OrderEquipment
                        {
                            EquipmentId = equipment.Id,
                            EquipmentName = equipment.Name,
                            CreatedAt = createdAt,
                        }
                    );
                }

                string code = Generator.GenerateCode("OD", 6);
                var deliveryTime = createdAt.AddDays(random.Next(1, 4));
                var order = new Order(
                    branchId: tariffChung.BranchId,
                    staffId: staffId,
                    code: code,
                    amount: amount,
                    total: total,
                    status: OrderStatus.Pending,
                    customerId: customer.Id,
                    tariffId: tariffChung.Id,
                    deliveryTime: deliveryTime,
                    voucherId: selectedVoucher?.Id,
                    voucherCode: selectedVoucher?.Code,
                    vat: vatPercent,
                    vatAmount: vatAmount,
                    discountFixed: discountFixed,
                    discountValue: discountValue,
                    point: point,
                    note: random.Next(0, 2) == 0 ? null : $"{code}"
                )
                {
                    CreatedAt = createdAt,
                    CodeConfirm = barcode.GenerateQrBase64(encryption.Encrypt(code)),
                    PaymentMethod = paymentMethods[orderIndex % paymentMethods.Length],
                    OrderDate = deliveryTime,
                };

                order.OrderItems.AddRangeSafe(orderItems);
                order.OrderEquipments.AddRangeSafe(orderEquipments);
                orders.Add(order);

                // Create InventoryDocument for export when order is completed
                if (order.Status == OrderStatus.Completed)
                {
                    var issueLines = orderItems
                        .SelectMany(oi =>
                        {
                            var unitRelation = services
                                .SelectMany(s => s.UnitRelations)
                                .FirstOrDefault(ur => ur.Id == oi.UnitRelationId);
                            if (unitRelation == null)
                            {
                                logger.Warning(
                                    $"No UnitRelation found for UnitRelation ID {oi.UnitRelationId}."
                                );
                                return Enumerable.Empty<IssueLine>();
                            }

                            if (!unitRelation.AsUnitProduct.Any())
                            {
                                logger.Warning(
                                    $"No ServiceResource found for UnitRelation ID {oi.UnitRelationId}."
                                );
                                return Enumerable.Empty<IssueLine>();
                            }

                            return unitRelation.AsUnitProduct.Select(sr =>
                            {
                                decimal serviceFactor = unitRelation.BaseUnit
                                    ? 1m
                                    : (decimal)unitRelation.Multiple;
                                decimal requireQty = sr.Quantity * serviceFactor * oi.Quantity;

                                return new IssueLine(
                                    sr.BranchProductId,
                                    sr.UnitProductId, // Use UnitProductId for material unit
                                    requireQty,
                                    sr.UnitProductPrice
                                );
                            });
                        })
                        .GroupBy(x => new { x.BranchProductId, x.UnitRelationId })
                        .Select(g => new IssueLine(
                            g.Key.BranchProductId,
                            g.Key.UnitRelationId,
                            g.Sum(x => x.Quantity),
                            g.First().Price
                        ))
                        .ToList();

                    if (issueLines.Any())
                    {
                        decimal totalProductAmount = issueLines.Sum(x => x.Price * x.Quantity);
                        var exportDocument = new InventoryDocument(
                            code: Generator.GenerateCode("XH", 6),
                            amount: totalProductAmount,
                            type: InventoryType.Export,
                            branchId: tariffChung.BranchId,
                            note: $"Phiếu xuất cho đơn hàng #{code}"
                        )
                        {
                            TransactionAt = createdAt,
                            CreatedAt = createdAt,
                        };

                        exportDocument.ProductSupplyings.AddRangeSafe(
                            issueLines
                                .Select(x => new ProductSupplying
                                {
                                    ProductId = x.BranchProductId,
                                    UnitRelationId = x.UnitRelationId,
                                    Price = x.Price,
                                    SupplierId = null,
                                    Quantity = -(int)Math.Ceiling(x.Quantity),
                                    CreatedAt = createdAt,
                                })
                                .ToList()
                        );

                        await unitOfWork
                            .Repository<InventoryDocument>()
                            .AddAsync(exportDocument, cancellationToken);
                        await unitOfWork.SaveAsync(cancellationToken);
                        exportDocument.UpdateStatus(InventoryStatus.Completed);
                        await unitOfWork.SaveAsync(cancellationToken);

                        logger.Information($"Created export inventory document for order {code}.");
                    }
                    else
                    {
                        logger.Warning(
                            $"No materials required for order {code}. Skipping export document."
                        );
                    }
                }

                orderIndex++;
            }

            currentDate = currentDate.AddDays(1);
        }

        try
        {
            logger.Information($"Initialized {orderIndex} orders from 2025-01-01 to 2025-08-12.");
            foreach (var item in orders)
            {
                await unitOfWork.Repository<Order>().AddAsync(item, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);
                logger.Information(
                    $"Initialized {orderIndex} orders from 2025-01-01 to 2025-08-12."
                );
                Order? orderEvent = await unitOfWork
                    .DynamicReadOnlyRepository<Order>()
                    .FindByConditionAsync(
                        new GetOrderByIdSpecification(item.Id),
                        cancellationToken
                    );
                if (orderEvent != null)
                {
                    orderEvent.UpdateStatus(OrderStatus.InProgress);
                    orderEvent.UpdateStatus(OrderStatus.Processed);
                    orderEvent.UpdateStatus(OrderStatus.Completed);
                    await unitOfWork.Repository<Order>().UpdateAsync(orderEvent);
                    await unitOfWork.SaveAsync(cancellationToken);
                }
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
        {
            logger.Error(
                ex,
                "Database error while saving orders: {ErrorMessage}",
                pgEx.MessageText
            );
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error while saving orders.");
            throw;
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
        var units = await unitOfWork
            .Repository<Domain.Aggregates.Services.Unit>()
            .ListAsync(cancellationToken);

        if (!suppliers.Any() || !products.Any() || !units.Any())
        {
            logger.Warning("Thiếu nhà cung cấp, sản phẩm hoặc đơn vị tính.");
            return;
        }

        var supplier = suppliers.First();

        // Tháng bắt đầu là tháng 1/2025
        var startDate = new DateTime(2025, 1, 1);
        var now = DateTime.Now;

        // Danh sách thiết bị mẫu để nhập
        var equipmentTemplates = new[]
        {
            new
            {
                Name = "Máy Giặt Công Nghiệp",
                CodePrefix = "EQ",
                UnitPrice = 20000000m,
                InitialQuantity = 10,
            },
            new
            {
                Name = "Máy Sấy Công Nghiệp",
                CodePrefix = "EQ",
                UnitPrice = 15000000m,
                InitialQuantity = 5,
            },
            new
            {
                Name = "Bàn Ủi Điện",
                CodePrefix = "EQ",
                UnitPrice = 5000000m,
                InitialQuantity = 2,
            },
        };

        var random = new Random();

        var currentMonth = new DateTime(startDate.Year, startDate.Month, 1);
        var endMonth = new DateTime(now.Year, now.Month, 1);

        while (currentMonth <= endMonth)
        {
            bool isFirstMonth = currentMonth == startDate;

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

                int quantity = isFirstMonth ? 100 : random.Next(200, 500);

                decimal amount = unitRelation.Price * quantity;
                totalProductAmount += amount;

                productSupplyings.Add(
                    new ProductSupplying
                    {
                        ProductId = product.Id,
                        SupplierId = supplier.Id,
                        Quantity = quantity,
                        Price = unitRelation.Price,
                        UnitRelationId = unitRelation.Id,
                        CreatedAt = currentMonth,
                    }
                );
            }

            decimal totalEquipmentAmount = 0;
            var equipmentSupplyings = new List<EquipmentSupplying>();

            if (isFirstMonth)
            {
                foreach (var eq in equipmentTemplates)
                {
                    int quantity = eq.InitialQuantity;

                    equipmentSupplyings.Add(
                        new EquipmentSupplying
                        {
                            Name = eq.Name,
                            Code = Generator.GenerateCode(eq.CodePrefix, 6),
                            Price = eq.UnitPrice,
                            Quantity = quantity,
                            SupplierId = supplier.Id,
                            CreatedAt = currentMonth,
                        }
                    );

                    totalEquipmentAmount += eq.UnitPrice * quantity;
                }
            }
            else
            {
                totalEquipmentAmount = 0;
            }

            var document = new InventoryDocument(
                code: Generator.GenerateCode("IM", 6),
                amount: totalProductAmount + totalEquipmentAmount,
                type: InventoryType.Import,
                branchId: 4,
                note: $"Phiếu nhập hàng tháng {currentMonth:MM/yyyy}"
            )
            {
                TransactionAt = currentMonth,
                CreatedAt = currentMonth,
            };

            document.ProductSupplyings.AddRangeSafe(productSupplyings);
            document.EquipmentSupplyings.AddRangeSafe(equipmentSupplyings);

            await unitOfWork.Repository<InventoryDocument>().AddAsync(document, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);

            document.UpdateStatus(InventoryStatus.Completed);
            await unitOfWork.SaveAsync(cancellationToken);

            currentMonth = currentMonth.AddMonths(1);
        }
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

public record IssueLine(long BranchProductId, long UnitRelationId, decimal Quantity, decimal Price);
