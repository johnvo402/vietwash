using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Command.UpdateStatus;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Contracts.Dtos.Requests;
using Contracts.Utils;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using Domain.Aggregates.Suppliers;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Domain.Aggregates.Vouchers;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        if (!provider.GetRequiredService<IHostEnvironment>().IsDevelopment())
            return;

        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        var encryption = provider.GetRequiredService<IEncryptionService>();
        var qrGenerator = provider.GetRequiredService<IQrGenerator>();
        var logger = provider.GetRequiredService<ILogger>();

        using var dbTransaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

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

            await InitializeInventoryDocumentsAsync(unitOfWork, logger, cancellationToken);
            await EnsureSeedEquipmentsAsync(unitOfWork, logger, cancellationToken);
            if (
                !await unitOfWork
                    .Repository<Service>()
                    .AnyAsync(cancellationToken: cancellationToken)
            )
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu dịch vụ...");
                await InitializeServicesAsync(unitOfWork, logger, cancellationToken);
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

            await ValidateSeedProductStockAsync(unitOfWork, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Lỗi xảy ra trong khi khởi tạo dữ liệu: {Message}", ex.Message);
            try
            {
                await unitOfWork.RollbackAsync(CancellationToken.None);
            }
            catch (Exception rollbackError)
            {
                logger.Error(rollbackError, "Development seed rollback failed; preserving the original seed error.");
            }
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
        CancellationToken cancellationToken
    )
    {
        long[] branchIds = { 1, 2, 3 };
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

        if (user == null)
            throw new InvalidOperationException("Development seed requires an Ecommerce admin projection from Auth synchronization. Start Auth/Project and synchronize users and branch assignments before retrying.");
        if (!categories.Any() || !units.Any())
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
            // Optional images are not a prerequisite for development data; no remote upload during seed.
            string? imageUrl = null;
            for (int i = 0; i < branchIds.Length; i++)
            {
                var products = (
                    await unitOfWork
                        .Repository<BranchProduct>()
                        .QueryAsync(x => x.BranchId == branchIds[i])
                        .Include(x => x.UnitRelations)
                        .ToListAsync(cancellationToken)
                ).ToDictionary(p => p.Name, p => p);
                if (!products.Any())
                    throw new InvalidOperationException("Missing Product data.");
                var service = new Service(
                    categoryId: categoryId,
                    branchId: branchIds[i],
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
                        logger.Warning(
                            $"Đơn vị '{unitName}' không tồn tại cho dịch vụ '{svc.Name}'."
                        );
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
                            throw new InvalidOperationException($"Seed service '{svc.Name}' is missing product '{productName}' in Branch {branchIds[i]}.");
                        }
                        var unitRelationEntity = branchProduct.UnitRelations.FirstOrDefault(x =>
                            resUnitName.Equals(x.Name, StringComparison.OrdinalIgnoreCase)
                        );

                        if (unitRelationEntity == null)
                        {
                            throw new InvalidOperationException($"Seed service '{svc.Name}' has no material unit '{resUnitName}' for '{productName}' in Branch {branchIds[i]}.");
                        }

                        var serviceResource = new ServiceResource
                        {
                            ProductId = branchProduct.Id,
                            UnitProductId = unitRelationEntity.Id,
                            UnitRelationId = unitRelation.Id,
                            Quantity = quantity,
                            CreatedAt = service.CreatedAt,
                        };

                        unitRelation.AsUnitRelation.Add(serviceResource);
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
        foreach (var item in laundryMaterials)
        {
            foreach (var branchId in new[] { 1, 2, 3 })
            {
                var product = new BranchProduct(
                    branchId: branchId,
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
                    ).ToUniversalTime(),
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
                            UnitId = unitInfo.IsBaseUnit
                                ? units[unitInfo.UnitName].Id
                                : (long?)null,
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
                    s.BranchId,
                    UnitRelations = s
                        .UnitRelations.Where(x =>
                            x.Status == ActivationStatus.Active && x.Price > 0
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
                DateTimeOffset.UtcNow.AddYears(1)
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

            bool isAugustTariff = name == "Bảng giá tháng 8";
            decimal discount = isAugustTariff ? 0.8m : 1.0m;

            foreach (var branchId in new[] { 1, 2, 3 })
            {
                var tariff = new Tariff(
                    name: name,
                    branchId: branchId,
                    status: startAt <= DateTimeOffset.UtcNow
                        ? ActivationStatus.Active
                        : ActivationStatus.Inactive,
                    startAt: startAt,
                    endAt: endAt
                )
                {
                    CreatedAt = startAt.AddDays(-1),
                };

                foreach (var service in validServices.Where(s => s.BranchId == branchId))
                {
                    var unitRelations = service
                        .UnitRelations.OrderBy(ur => ur.ProcessingTime)
                        .ToList();

                    foreach (var ur in unitRelations)
                    {
                        tariff.ServiceTariffs.Add(
                            new ServiceTariff
                            {
                                TariffId = tariff.Id,
                                ServiceId = service.Id,
                                UnitRelationId = ur.Id,
                                Price = ur.Price * discount,
                                CreatedAt = DateTimeOffset.UtcNow,
                            }
                        );
                        serviceTariffsCreated++;
                    }
                }

                if (tariff.ServiceTariffs.Any())
                {
                    await unitOfWork.Repository<Tariff>().AddAsync(tariff, cancellationToken);
                    tariffsCreated++;
                    logger.Information(
                        "Created tariff {TariffName} for branch {BranchId} with {Count} service tariffs.",
                        name,
                        branchId,
                        tariff.ServiceTariffs.Count
                    );
                }
            }

            await unitOfWork.SaveAsync(cancellationToken);
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
                imgUrl: "voucher.1.webp",
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
        var customers = await unitOfWork.Repository<User>()
            .QueryAsync(x => x.Role == ROLE.CUSTOMER && !x.Disabled && x.Status == ActivationStatus.Active)
            .OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var staff = await unitOfWork.Repository<User>()
            .QueryAsync(x => x.Role == ROLE.STAFF && !x.Disabled && x.Status == ActivationStatus.Active)
            .Include(x => x.BranchUsers).OrderBy(x => x.Id).ToListAsync(cancellationToken);
        if (customers.Count == 0)
            throw new InvalidOperationException("Development seed requires an active Ecommerce customer from Auth synchronization.");

        var equipments = await unitOfWork.Repository<Equipment>().QueryAsync()
            .ToListAsync(cancellationToken);
        var reserved = equipments.Where(x => x.Using).Select(x => x.Id).ToHashSet();
        var seededOrders = new List<Order>();
        OrderStatus[] states = [
            OrderStatus.Completed, OrderStatus.Completed,
            OrderStatus.Processed, OrderStatus.Processed,
            OrderStatus.InProgress, OrderStatus.InProgress, OrderStatus.Pending
        ];
        DateTimeOffset now = DateTimeOffset.UtcNow;
        foreach (long branchId in DevelopmentSeedPolicy.BranchIds)
        {
            var branchStaff = staff.FirstOrDefault(x => x.BranchUsers!.Any(b => b.BranchId == branchId))
                ?? throw new InvalidOperationException($"Development seed requires active staff assigned to Branch {branchId}.");
            var tariff = await unitOfWork.Repository<Tariff>()
                .QueryAsync(x => x.BranchId == branchId && x.Name == "Bảng giá chung" && x.Status == ActivationStatus.Active)
                .Include(x => x.ServiceTariffs).FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException($"No active common seed tariff exists for Branch {branchId}.");
            var services = await unitOfWork.Repository<Service>()
                .QueryAsync(x => x.BranchId == branchId && x.Status == ActivationStatus.Active && !x.Disable)
                .Include(x => x.UnitRelations).OrderBy(x => x.Name).ToListAsync(cancellationToken);
            var choices = services.SelectMany(s => s.UnitRelations
                .Where(u => u.Status == ActivationStatus.Active && u.Price > 0)
                .Select(u => new
                {
                    Service = s,
                    Unit = u,
                    Price = tariff.ServiceTariffs
                    .FirstOrDefault(t => t.ServiceId == s.Id && t.UnitRelationId == u.Id)?.Price
                }))
                .Where(x => x.Price > 0).ToList();
            if (choices.Count == 0)
                throw new InvalidOperationException($"No usable seeded service/tariff exists for Branch {branchId}.");

            var random = new Random(100 + (int)branchId);
            for (int index = 0; index < states.Length; index++)
            {
                var choice = choices[index % choices.Count];
                var createdAt = now.AddDays(index - states.Length + 1);
                const int quantity = 3;
                const int vat = 10;
                decimal amount = choice.Price!.Value * quantity;
                decimal vatAmount = amount * vat / 100;
                string code = $"DEV-OD-B{branchId}-{index + 1:D2}";
                // These are explicit historical fixtures, not operational transitions:
                // do not emit accounting, invoice, notification or voucher events.
                var order = new Order(branchId, branchStaff.Id, code, amount, amount + vatAmount,
                    states[index], vat: vat, vatAmount: vatAmount, customerId: customers[index % customers.Count].Id,
                    tariffId: tariff.Id, note: DevelopmentSeedPolicy.OrderNote, deliveryTime: createdAt.AddDays(2))
                {
                    CreatedAt = createdAt,
                    CodeConfirm = barcode.GenerateQrBase64(encryption.Encrypt(code)),
                    PaymentMethod = states[index] == OrderStatus.Completed ? PaymentMethod.Cash : null,
                    OrderDate = states[index] == OrderStatus.Completed ? createdAt.AddDays(1) : null,
                };
                order.OrderItems.Add(new OrderItem
                {
                    ServiceId = choice.Service.Id,
                    ServiceName = choice.Service.Name,
                    UnitRelationId = choice.Unit.Id,
                    UnitRelationName = choice.Unit.Name,
                    ProcessingTime = (int)choice.Unit.ProcessingTime,
                    Quantity = quantity,
                    Price = choice.Price.Value,
                    UnitPrice = choice.Price.Value,
                    CreatedAt = createdAt,
                });
                foreach (Equipment equipment in DevelopmentSeedPolicy.SelectEquipment(branchId, order.Status, equipments, reserved, random))
                    order.OrderEquipments.Add(new OrderEquipment
                    {
                        EquipmentId = equipment.Id,
                        EquipmentName = equipment.Name,
                        CreatedAt = createdAt,
                    });
                seededOrders.Add(order);
            }
        }
        DevelopmentSeedPolicy.ValidateOrders(seededOrders, equipments);
        await unitOfWork.Repository<Order>().AddRangeAsync(seededOrders, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        foreach (Order order in seededOrders.Where(x => x.Status != OrderStatus.Pending))
        {
            // Reuse the existing resolver/stock validator/export factory and SourceOrderId idempotency.
            var result = await OrderMaterialConsumption.ConsumeAsync(unitOfWork, order, cancellationToken);
            if (!result.IsSuccess)
                throw new InvalidOperationException($"Seed order {order.Code}: {result.ErrorMessage}");
            if (result.ExportDocument is not null)
            {
                result.ExportDocument.CreatedAt = order.CreatedAt;
                result.ExportDocument.TransactionAt = order.CreatedAt;
            }
            await unitOfWork.SaveAsync(cancellationToken);
        }
        logger.Information("Created {Count} branch-scoped development orders without operational events.", seededOrders.Count);
    }

    private static async Task InitializeInventoryDocumentsAsync(
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        var supplier = await unitOfWork.Repository<Supplier>().QueryAsync()
            .OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Development inventory seed requires a supplier.");
        var existing = (await unitOfWork.Repository<InventoryDocument>()
            .QueryAsync(x => x.Type == InventoryType.Import && x.Status == InventoryStatus.Completed)
            .ToListAsync(cancellationToken)).Where(DevelopmentSeedPolicy.IsSeedImport).ToList();
        var months = existing.Where(x => x.TransactionAt.HasValue)
            .Select(x => (x.BranchId, x.TransactionAt!.Value.Year, x.TransactionAt.Value.Month)).ToHashSet();
        var templates = new[]
        {
            (Name: "Máy Giặt Công Nghiệp", Code: "WM", Price: 20000000m, Quantity: 10, Image: "may-giat.1.jpg"),
            (Name: "Máy Sấy Công Nghiệp", Code: "DR", Price: 15000000m, Quantity: 5, Image: "may-say.1.jpg"),
            (Name: "Bàn Ủi Điện", Code: "IR", Price: 5000000m, Quantity: 2, Image: "banui.jpg"),
        };
        var firstMonth = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var now = DateTimeOffset.UtcNow;
        foreach (long branchId in DevelopmentSeedPolicy.BranchIds)
        {
            var products = await unitOfWork.Repository<BranchProduct>()
                .QueryAsync(x => x.BranchId == branchId && x.Status == ActivationStatus.Active && !x.Disable
                    && DevelopmentSeedPolicy.ProductNames.Contains(x.Name)
                    && x.Description == "Vật tư cửa hàng giặt ủi: " + x.Name)
                .Include(x => x.UnitRelations).ToListAsync(cancellationToken);
            if (products.Count == 0)
                throw new InvalidOperationException($"No active seed products exist for Branch {branchId}.");
            // Reset the month for EACH branch; an existing receipt owns its lines and is never duplicated.
            for (var month = firstMonth; month <= now; month = month.AddMonths(1))
            {
                if (months.Contains((branchId, month.Year, month.Month)))
                    continue;
                var productLines = products.Select(product =>
                {
                    var unit = product.UnitRelations.FirstOrDefault(x => x.BaseUnit && x.Multiple == 1 && x.Status == ActivationStatus.Active)
                        ?? throw new InvalidOperationException($"Seed product '{product.Name}' in Branch {branchId} has no active base unit.");
                    return new ProductSupplying
                    {
                        ProductId = product.Id,
                        UnitRelationId = unit.Id,
                        SupplierId = supplier.Id,
                        Quantity = month == firstMonth ? 100 : 300,
                        Price = product.CapitalPrice,
                        CreatedAt = month,
                    };
                }).ToList();
                var equipmentLines = month == firstMonth
                    ? templates.Select(t => new EquipmentSupplying
                    {
                        Name = t.Name,
                        Code = $"DEV-B{branchId}-{t.Code}",
                        Price = t.Price,
                        Quantity = t.Quantity,
                        SupplierId = supplier.Id,
                        Image = t.Image,
                        CreatedAt = month,
                    }).ToList() : [];
                var document = new InventoryDocument(
                    $"DEV-IM-B{branchId}-{month:yyyyMM}",
                    productLines.Sum(x => x.Price * x.Quantity) + equipmentLines.Sum(x => x.Price * x.Quantity),
                    InventoryType.Import, branchId, $"Phiếu nhập hàng tháng {month:MM/yyyy}")
                {
                    // Fixture state only: NEVER call UpdateStatus and dispatch runtime completion here.
                    Status = InventoryStatus.Completed,
                    TransactionAt = month,
                    CreatedAt = month,
                    ProductSupplyings = productLines,
                    EquipmentSupplyings = equipmentLines,
                };
                await unitOfWork.Repository<InventoryDocument>().AddAsync(document, cancellationToken);
            }
        }
        await unitOfWork.SaveAsync(cancellationToken);
        logger.Information("Development inventory receipts reconciled for branches 1, 2 and 3.");
    }

    public static async Task EnsureSeedEquipmentsAsync(
        IUnitOfWork unitOfWork, ILogger logger, CancellationToken cancellationToken = default)
    {
        var documents = await unitOfWork.Repository<InventoryDocument>()
            .QueryAsync(x => x.Type == InventoryType.Import && x.Status == InventoryStatus.Completed)
            .Include(x => x.EquipmentSupplyings).ToListAsync(cancellationToken);
        var existing = await unitOfWork.Repository<Equipment>().QueryAsync().ToListAsync(cancellationToken);
        var missing = DevelopmentSeedPolicy.MissingEquipment(documents, existing, DateTimeOffset.UtcNow);
        if (missing.Count == 0)
            return;
        await unitOfWork.Repository<Equipment>().AddRangeAsync(missing, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        logger.Information("Reconciled {Count} missing development seed equipment.", missing.Count);
    }

    private static async Task ValidateSeedProductStockAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var services = await unitOfWork.Repository<Service>()
            .QueryAsync(x => DevelopmentSeedPolicy.BranchIds.Contains(x.BranchId) && x.Status == ActivationStatus.Active && !x.Disable)
            .Include(x => x.UnitRelations).ThenInclude(x => x.AsUnitRelation).ThenInclude(x => x.BranchProduct)
            .Include(x => x.UnitRelations).ThenInclude(x => x.AsUnitRelation).ThenInclude(x => x.UnitProduct)
            .ToListAsync(cancellationToken);
        var stocks = await unitOfWork.Repository<ProductSupplying>()
            .QueryAsync(x => x.InventoryDocument.Status == InventoryStatus.Completed)
            .GroupBy(x => x.ProductId)
            .Select(x => new MaterialStockSnapshot(x.Key, x.Sum(s => s.Quantity * s.UnitRelation.Multiple)))
            .ToListAsync(cancellationToken);
        foreach (long branchId in DevelopmentSeedPolicy.BranchIds)
        {
            var branchServices = services.Where(x => x.BranchId == branchId).ToArray();
            if (branchServices.Length == 0)
                throw new InvalidOperationException($"No active seed services exist for Branch {branchId}.");
            bool hasMaterials = false;
            foreach (var service in branchServices)
            {
                var inputs = service.UnitRelations.Where(u => u.Status == ActivationStatus.Active)
                    .SelectMany(u => u.AsUnitRelation.Select(r => new OrderMaterialInput(
                        service.Id, u.ServiceId, u.Status, u.BaseUnit, u.Multiple, 5,
                        r.ProductId, r.BranchProduct.Name, r.BranchProduct.BranchId, r.BranchProduct.Status,
                        r.BranchProduct.Disable, r.BranchProduct.CapitalPrice, r.UnitProductId,
                        r.UnitProduct.BranchProductId, r.UnitProduct.Status, r.UnitProduct.BaseUnit,
                        r.UnitProduct.Multiple, r.Quantity))).ToArray();
                var resolution = OrderMaterialRequirementResolver.Resolve(branchId, inputs);
                if (!resolution.IsSuccess)
                    throw new InvalidOperationException($"Seed service '{service.Name}', Branch {branchId}: {resolution.ErrorMessage}");
                hasMaterials |= resolution.Requirements.Count > 0;
                var stock = OrderMaterialStockValidator.Validate(resolution.Requirements, stocks);
                if (!stock.IsSuccess)
                    throw new InvalidOperationException($"Seed stock for Branch {branchId}: {stock.ErrorMessage}");
            }
            if (!hasMaterials)
                throw new InvalidOperationException($"Seed services in Branch {branchId} have no material resource definitions.");
        }
    }

}
