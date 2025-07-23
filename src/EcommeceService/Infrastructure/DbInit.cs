using System.Linq.Expressions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Contracts.Dtos.Requests;
using Contracts.Utils;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Enums;
using Domain.Aggregates.Services.Specifications;
using Domain.Aggregates.Suppliers;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Domain.Aggregates.Vouchers;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Specification;

namespace Infrastructure.Data;

public class DbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider provider,
        CancellationToken cancellationToken = default
    )
    {
        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        var encryption = provider.GetRequiredService<IEncryptionService>();
        var qrGenerator = provider.GetRequiredService<IQrGenerator>();
        var logger = provider.GetRequiredService<ILogger>();
        using var dbTransaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            if (!await unitOfWork.Repository<Unit>().AnyAsync())
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu đơn vị tính...");

                await InitializeUnitsAsync(unitOfWork, cancellationToken);

                logger.Information("Hoàn tất khởi tạo dữ liệu đơn vị tính...");
            }
            else
            {
                logger.Information("Dữ liệu đơn vị tính đã tồn tại, bỏ qua khởi tạo.");
            }
            if (!await unitOfWork.Repository<Category>().AnyAsync())
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu danh mục...");

                await InitializeCategoriesAsync(unitOfWork, cancellationToken);

                logger.Information("Hoàn tất khởi tạo dữ liệu danh mục...");
            }
            else
            {
                logger.Information("Dữ liệu danh mục đã tồn tại, bỏ qua khởi tạo.");
            }

            if (!await unitOfWork.Repository<Supplier>().AnyAsync())
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu nhà cung cấp...");

                await InitializeSuppliersAsync(unitOfWork, cancellationToken);

                logger.Information("Hoàn tất khởi tạo dữ liệu nhà cung cấp...");
            }
            else
            {
                logger.Information("Dữ liệu nhà cung cấp đã tồn tại, bỏ qua khởi tạo.");
            }
            if (!await unitOfWork.Repository<Service>().AnyAsync())
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu dịch vụ...");

                await InitializeServicesAsync(unitOfWork, logger, cancellationToken);

                logger.Information("Hoàn tất khởi tạo dữ liệu dịch vụ...");
            }
            else
            {
                logger.Information("Dữ liệu dịch vụ đã tồn tại, bỏ qua khởi tạo.");
            }
            if (!await unitOfWork.Repository<Voucher>().AnyAsync())
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu voucher...");

                await InitVouchersAsync(unitOfWork, qrGenerator, cancellationToken);

                logger.Information("Hoàn tất khởi tạo dữ liệu voucher...");
            }
            // Initialize Orders
            if (!await unitOfWork.Repository<Order>().AnyAsync())
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu đơn hàng...");

                await InitializeOrdersAsync(
                    unitOfWork,
                    logger,
                    cancellationToken,
                    encryption,
                    qrGenerator
                );

                logger.Information("Hoàn tất khởi tạo dữ liệu đơn hàng...");
            }
            else
            {
                logger.Information("Dữ liệu đơn hàng đã tồn tại, bỏ qua khởi tạo.");
            }
            if (!await unitOfWork.Repository<BranchProduct>().AnyAsync())
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu sản phẩm chi nhánh...");

                await InitializeBranchProductsAsync(unitOfWork, logger, cancellationToken);

                logger.Information("Hoàn tất khởi tạo dữ liệu sản phẩm chi nhánh...");
            }
            if (!await unitOfWork.Repository<InventoryDocument>().AnyAsync())
            {
                logger.Information("Bắt đầu khởi tạo phiếu nhập kho...");

                await InitializeInventoryDocumentsAsync(unitOfWork, logger, cancellationToken);

                logger.Information("Hoàn tất khởi tạo phiếu nhập kho.");
            }

            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.Error("Lỗi xảy ra trong khi khởi tạo dữ liệu đơn hàng: {Message}", ex.Message);
            throw;
        }
    }

    private static Expression<Func<User, ListIds>> SelectOnlyId() =>
        user => new ListIds { Id = user.Id };

    private static async Task InitializeOrdersAsync(
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken,
        IEncryptionService encryption,
        IQrGenerator barcode
    )
    {
        // Fetch customer IDs
        var customerResult = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .ListAsync(
                new ListUserSpecification([ROLE.CUSTOMER]),
                new QueryParamRequest { },
                SelectOnlyId(),
                cancellationToken
            );

        if (!customerResult.Any())
        {
            logger.Warning("Không tìm thấy khách hàng trong cơ sở dữ liệu.");
            return;
        }

        // Fetch staff IDs
        var staffResult = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .ListAsync(
                new ListUserSpecification([ROLE.STAFF]),
                new QueryParamRequest { },
                SelectOnlyId(),
                cancellationToken
            );

        if (!staffResult.Any())
        {
            logger.Warning("Không tìm thấy nhân viên trong cơ sở dữ liệu.");
            return;
        }

        var random = new Random();
        var paymentMethods = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>().ToArray();

        for (int i = 1; i <= 50; i++)
        {
            long? customerId = customerResult[random.Next(customerResult.Count())].Id;
            long staffId = staffResult[random.Next(staffResult.Count())].Id;

            int itemCount = random.Next(1, 4); // Số lượng mục trong đơn hàng
            var orderItems = new List<OrderItem>();
            decimal totalPrice = 0;
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
                logger.Warning("Không tìm thấy dịch vụ trong cơ sở dữ liệu.");
                continue;
            }

            for (int j = 0; j < itemCount; j++)
            {
                var service = services.OrderBy(x => random.Next()).First();
                long serviceId = service.Id;
                string serviceName = service.Name;

                var unitRelations = service
                    .UnitRelations.Where(x =>
                        x.Status.Equals(ActivationStatus.Active) && x.Price > 0
                    )
                    .ToList();

                if (!unitRelations.Any())
                {
                    logger.Warning($"Không tìm thấy unit_relation cho dịch vụ ID {serviceId}.");
                    continue;
                }

                var unitRelation = unitRelations.OrderBy(x => random.Next()).First();
                long unitRelationId = unitRelation.Id;
                string unitRelationName = unitRelation.Name;
                int processingTime = (int)unitRelation.ProcessingTime;
                decimal unitPrice = unitRelation.Price;

                if (unitPrice == 0)
                {
                    logger.Warning(
                        $"Giá đơn vị cho unit_relation ID {unitRelationId} không hợp lệ."
                    );
                    continue;
                }

                int quantity = 1 + random.Next(3);

                var orderItem = new OrderItem
                {
                    ServiceId = serviceId,
                    UnitRelationId = unitRelationId,
                    Price = quantity * unitPrice,
                    Quantity = quantity,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UnitRelationName = unitRelationName,
                    ProcessingTime = processingTime,
                    ServiceName = serviceName,
                    UnitPrice = unitPrice,
                };

                orderItems.Add(orderItem);
                totalPrice += orderItem.Price;
            }

            if (!orderItems.Any())
            {
                logger.Warning($"Không tạo được OrderItem cho đơn hàng {i}.");
                continue;
            }

            string code = Generator.GenerateCode("OD", 6);

            var statusValues = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Where(s => s != OrderStatus.Cancelled)
                .ToArray();

            OrderStatus status =
                statusValues.Length > 0
                    ? statusValues[random.Next(statusValues.Length)]
                    : OrderStatus.Pending;

            var order = new Order(
                branchId: 1,
                staffId: staffId,
                code: code,
                amount: totalPrice,
                total: totalPrice,
                status: status,
                orderDate: DateTimeOffset.UtcNow.AddDays(-random.NextDouble() * 10),
                customerId: customerId,
                discountFixed: true,
                discountValue: 0,
                note: $"Đơn hàng tự động {i}",
                deliveryTime: DateTimeOffset.UtcNow.AddDays(1)
            );
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
                var paymentMethod = paymentMethods[random.Next(paymentMethods.Length)];
                order.PaymentMethod = paymentMethod;
            }

            await unitOfWork.Repository<Order>().AddAsync(order, cancellationToken);
            order.UpdateStatus(status);
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
                code: "DMX",
                status: ActivationStatus.Active,
                email: "cskh@thegioididong.com",
                address: "172B Đường 3/2, Phường Hưng Lợi, Quận Ninh Kiều, Thành phố Cần Thơ",
                phone: "02838125960",
                description: "Siêu thị Điện máy XANH tại 172B Đường 3/2, Phường Hưng Lợi, Quận Ninh Kiều, TP. Cần Thơ chính thức khai trương từ ngày 10/07/2015, mang đến không gian mua sắm hiện đại với hàng ngàn sản phẩm chính hãng đa dạng từ điện lạnh, gia dụng đến điện tử, viễn thông. Khách hàng sẽ được trải nghiệm dịch vụ chuyên nghiệp với đội ngũ nhân viên thân thiện, tư vấn tận tình, chính sách trả góp linh hoạt, giao hàng và lắp đặt tận nơi, cùng chương trình bảo hành – đổi trả hấp dẫn. Ngoài ra, siêu thị còn hỗ trợ thanh toán qua thẻ và có khu vực để xe tiện lợi với bảo vệ phục vụ chu đáo. Điện máy XANH cam kết mang đến sản phẩm chất lượng, giá tốt và trải nghiệm mua sắm tối ưu cho khách hàng."
            )
            {
                Disable = false,
            },
            new Supplier(
                name: "Siêu Thị Điện Máy Chợ Lớn",
                code: "STDMCL",
                status: ActivationStatus.Active,
                email: "dienmaycantho2@dienmaycholon.com.vn",
                address: "161 Đường 3/2, Phường Hưng Lợi, Quận Ninh Kiều, TP.Cần Thơ",
                phone: "02839505060",
                description: "Với hơn 20 năm kinh nghiệm trong lĩnh vực kim khí điện máy, có trụ sở chính đặt tại Quận 5 và hệ thống chi nhánh trên toàn quốc Siêu Thị Điện Máy – Nội Thất Chợ Lớn chi nhánh Ninh Kiều, Cần Thơ được đặt tại địa chỉ số 161 Đường 3/2, P. Hưng Lợi, Q. Ninh Kiều, TP. Cần Thơ với tổng diện tích là 1100m2 cho không gian trưng bày rộng lớn với các quầy kệ trưng bày đa dạng các sản phẩm tiện ích mang đến Quý khách hàng trải nghiệm mua sắm đích thực."
            )
            {
                Disable = false,
            },
            new Supplier(
                name: "Siêu Thị GO! VIETNAM",
                code: "STG",
                status: ActivationStatus.Active,
                email: "crv.dvkh@vn.centralretail.com",
                address: "Lô số 1, KDC Hưng Phú 1, Phường Hưng Phú, Quận Cái Răng, TP. Cần Thơ",
                phone: "02923737575",
                description: "Là đơn vị tiên phong trong lĩnh vực cung cấp và setup hệ thống giặt là cho khách sạn , xưởng giặt là , bệnh viện cho nên chúng tôi luôn có chất lượng sản phẩm tốt , giá thành cạnh tranh trên thị trường hiện nay ."
            )
            {
                Disable = false,
            },
        };

        foreach (var supplier in suppliers)
        {
            await unitOfWork.Repository<Supplier>().AddAsync(supplier, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
        }
    }

    private static async Task InitializeCategoriesAsync(
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken
    )
    {
        var categories = new List<Category>();

        // Root categories
        string code = Generator.GenerateCode("DM", 6);
        var giatCategory = new Category(
            name: "Giặt",
            parentId: null,
            status: ActivationStatus.Active,
            code: code
        )
        {
            Disabled = false,
            Path = code.ToLowerInvariant(),
        };
        categories.Add(giatCategory);

        code = Generator.GenerateCode("DM", 6);
        var uiCategory = new Category(
            name: "Ủi",
            parentId: null,
            status: ActivationStatus.Active,
            code: code
        )
        {
            Disabled = false,
            Path = code.ToLowerInvariant(),
        };
        categories.Add(uiCategory);

        code = Generator.GenerateCode("DM", 6);
        var sayCategory = new Category(
            name: "Sấy",
            parentId: null,
            status: ActivationStatus.Active,
            code: code
        )
        {
            Disabled = false,
            Path = code.ToLowerInvariant(),
        };
        categories.Add(sayCategory);

        code = Generator.GenerateCode("DM", 6);
        var veSinhCategory = new Category(
            name: "Vệ Sinh",
            parentId: null,
            status: ActivationStatus.Active,
            code: code
        )
        {
            Disabled = false,
            Path = code.ToLowerInvariant(),
        };
        categories.Add(veSinhCategory);

        code = Generator.GenerateCode("DM", 6);
        var comboCategory = new Category(
            name: "Combo",
            parentId: null,
            status: ActivationStatus.Active,
            code: code
        )
        {
            Disabled = false,
            Path = code.ToLowerInvariant(),
        };
        categories.Add(comboCategory);

        // Child categories
        code = Generator.GenerateCode("DM", 6);
        var giatDacBietCategory = new Category(
            name: "Giăt Đặc Biệt",
            parentId: null,
            status: ActivationStatus.Active,
            code: code
        )
        {
            Disabled = false,
            Path = code.ToLowerInvariant(),
        };
        categories.Add(giatDacBietCategory);

        code = Generator.GenerateCode("DM", 6);
        var nuocGiatCategory = new Category(
            name: "Nước giặt",
            parentId: null,
            status: ActivationStatus.Active,
            code: code
        )
        {
            Disabled = false,
            Path = code.ToLowerInvariant(),
        };
        categories.Add(nuocGiatCategory);

        code = Generator.GenerateCode("DM", 6);
        var nuocXaCategory = new Category(
            name: "Nước xả",
            parentId: null,
            status: ActivationStatus.Active,
            code: code
        )
        {
            Disabled = false,
            Path = code.ToLowerInvariant(),
        };
        categories.Add(nuocXaCategory);

        code = Generator.GenerateCode("DM", 6);
        var nuocVeSinhCategory = new Category(
            name: "Nước vệ sinh",
            parentId: null,
            status: ActivationStatus.Active,
            code: code
        )
        {
            Disabled = false,
            Path = code.ToLowerInvariant(),
        };
        categories.Add(nuocVeSinhCategory);
        foreach (var category in categories)
        {
            await unitOfWork.Repository<Category>().AddAsync(category, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
        }
    }

    private static async Task InitializeUnitsAsync(
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken
    )
    {
        var units = new List<Unit>
        {
            new Unit(name: "Kg", status: ActivationStatus.Active),
            new Unit(name: "Bộ", status: ActivationStatus.Active),
            new Unit(name: "Mét", status: ActivationStatus.Active),
            new Unit(name: "Lít", status: ActivationStatus.Active),
            new Unit(name: "Đôi", status: ActivationStatus.Active),
            new Unit(name: "Hộp", status: ActivationStatus.Active),
            new Unit(name: "Thùng", status: ActivationStatus.Active),
        };
        foreach (var unit in units)
        {
            await unitOfWork.Repository<Unit>().AddAsync(unit, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
        }
    }

    private static async Task InitializeServicesAsync(
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        // Query existing Categories, Branches, and Units
        var categories = (
            await unitOfWork.Repository<Category>().ListAsync(cancellationToken)
        ).ToList();
        var user = (
            await unitOfWork
                .DynamicReadOnlyRepository<User>()
                .FindByConditionAsync(
                    new ListUserSpecification([ROLE.ADMIN]),
                    cancellationToken: cancellationToken
                )
        );
        if (user == null)
        {
            logger.Error("Không tìm thấy người dùng có vai trò ADMIN.");
            throw new InvalidOperationException("Admin user not found.");
        }
        var units = (await unitOfWork.Repository<Unit>().ListAsync(cancellationToken)).ToList();

        if (!categories.Any() || !units.Any())
        {
            logger.Error("Không thể khởi tạo dịch vụ: Thiếu dữ liệu danh mục hoặc đơn vị tính.");
            throw new InvalidOperationException("Missing required Category, Branch, or Unit data.");
        }

        var random = new Random();
        var services = new List<Service>();

        for (int i = 1; i <= 20; i++)
        {
            // Chọn ngẫu nhiên Category, Branch và Unit
            var category = categories[random.Next(categories.Count)];
            var unit = units[random.Next(units.Count)];
            var type = random.Next(2) == 0 ? TypeStatus.SingleService : TypeStatus.Combo;

            // Định nghĩa tên dịch vụ và mô tả dựa trên loại dịch vụ
            string serviceName;
            string description;
            if (type == TypeStatus.SingleService)
            {
                string[] singleServiceNames =
                {
                    "Giặt và Gấp",
                    "Giặt Hấp",
                    "Ủi Áo Sơ Mi",
                    "Giặt Chăn Ga",
                    "Giặt Quần Áo Mỏng",
                    "Tẩy Vết Bẩn",
                    "Dịch Vụ Ủi",
                    "Giặt Rèm Cửa",
                    "Giặt Đồng Phục",
                    "Giặt Thảm",
                };
                string[] singleServiceDescriptions =
                {
                    "Giặt và gấp quần áo thông thường chuyên nghiệp.",
                    "Giặt hấp cho vải tinh xảo và đặc biệt.",
                    "Ủi áo sơ mi để có vẻ ngoài sắc nét.",
                    "Làm sạch kỹ lưỡng cho chăn, ga, gối.",
                    "Giặt nhẹ nhàng cho quần áo mỏng để bảo vệ chất lượng.",
                    "Xử lý vết bẩn cứng đầu trên quần áo.",
                    "Ủi quần áo cẩn thận để không còn nếp nhăn.",
                    "Làm sạch chuyên sâu cho rèm cửa.",
                    "Giặt và ủi đồng phục chuyên nghiệp.",
                    "Làm sạch sâu cho thảm trải sàn.",
                };
                int index = (i - 1) % singleServiceNames.Length;
                serviceName = singleServiceNames[index];
                description = singleServiceDescriptions[index];
            }
            else
            {
                string[] comboServiceNames =
                {
                    "Gói Giặt và Ủi",
                    "Gói Giặt Hấp và Ủi",
                    "Gói Giặt Toàn Diện",
                    "Gói Chăn Ga và Khăn",
                    "Gói Giặt Gia Đình",
                    "Gói Giặt Nhanh",
                    "Gói Giặt Hấp Cao Cấp",
                    "Gói Rèm và Chăn Ga",
                    "Gói Đồng Phục và Áo Sơ Mi",
                    "Gói Thảm và Nội Thất",
                };
                string[] comboServiceDescriptions =
                {
                    "Dịch vụ giặt và ủi toàn diện cho quần áo.",
                    "Giặt hấp và ủi cho vẻ ngoài chuyên nghiệp.",
                    "Dịch vụ giặt toàn bộ đồ dùng trong nhà.",
                    "Làm sạch chăn ga và khăn trong một gói.",
                    "Gói giặt cho số lượng lớn của gia đình.",
                    "Giặt và ủi nhanh chóng cho nhu cầu gấp.",
                    "Giặt hấp cao cấp cho quần áo cao cấp.",
                    "Làm sạch kết hợp cho rèm và chăn ga.",
                    "Làm sạch chuyên sâu cho đồng phục và áo sơ mi.",
                    "Làm sạch sâu cho thảm và nội thất bọc vải.",
                };
                int index = (i - 1) % comboServiceNames.Length;
                serviceName = comboServiceNames[index];
                description = comboServiceDescriptions[index];
            }
            var branch = 1L;
            // Tạo dịch vụ
            var service = new Service(
                categoryId: category.Id,
                branchId: branch,
                name: serviceName,
                type: type,
                status: ActivationStatus.Active,
                description: description,
                image: null
            )
            {
                Disable = false,
            };
            service.Slug = Generator.GenerateSlug(service.Name);

            // Thêm UnitRelation
            var unitRelation = new UnitRelation
            {
                Name = unit.Name,
                BaseUnit = true,
                Price = random.Next(1000, 100000),
                Multiple = 1,
                ProcessingTime = (decimal)(random.NextDouble() * 4.5 + 0.5),
                Status = ActivationStatus.Active,
            };
            service.UnitRelations.Add(unitRelation);

            services.Add(service);
        }

        await unitOfWork.Repository<Service>().AddRangeAsync(services, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
    }

    private static async Task InitializeBranchProductsAsync(
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        var categories = await unitOfWork.Repository<Category>().ListAsync(cancellationToken);
        var units = await unitOfWork.Repository<Unit>().ListAsync(cancellationToken);

        if (!categories.Any() || !units.Any())
        {
            logger.Warning("Thiếu danh mục hoặc đơn vị tính để khởi tạo sản phẩm.");
            return;
        }

        var branchId = 1L; // Chi nhánh mặc định
        var random = new Random();
        var products = new List<BranchProduct>();

        string[] productNames =
        {
            "Bột giặt Omo",
            "Nước xả Downy",
            "Túi giặt lưới",
            "Chất tẩy Javel",
            "Xịt thơm quần áo",
            "Nước giặt Ariel",
            "Viên giặt Tide",
            "Nước vệ sinh máy",
            "Chổi lông gà",
            "Găng tay cao su",
        };

        for (int i = 0; i < 10; i++)
        {
            var name = productNames[i];
            var sku = Generator.GenerateCode(10);
            var price = random.Next(10000, 100000);
            var categoriesList = categories.ToList();
            var categoryId = categoriesList[random.Next(categoriesList.Count)].Id;
            var unitList = units.ToList();
            var unit = unitList[random.Next(unitList.Count)];

            var product = new BranchProduct(
                branchId: branchId,
                name: name,
                sku: sku,
                status: ActivationStatus.Active,
                capitalPrice: price,
                categoryId: categoryId,
                description: $"Sản phẩm dùng trong giặt ủi: {name}",
                image: null
            );

            var unitRelation = new UnitRelation
            {
                Name = unit.Name,
                BaseUnit = true,
                Price = price + random.Next(1000, 5000),
                Multiple = 1,
                ProcessingTime = (decimal)(random.NextDouble() * 3 + 1), // 1 -> 4 giờ
                Status = ActivationStatus.Active,
                BranchProductId = null,
                ServiceId = null,
            };

            product.UnitRelations.Add(unitRelation);
            products.Add(product);
        }

        await unitOfWork.Repository<BranchProduct>().AddRangeAsync(products, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
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
        var random = new Random();

        if (!suppliers.Any() || !products.Any() || !units.Any())
        {
            logger.Warning("Thiếu nhà cung cấp, sản phẩm hoặc đơn vị tính.");
            return;
        }

        var supplierList = suppliers.ToList();
        var supplier = supplierList[random.Next(supplierList.Count)];
        var branchId = 1L;

        decimal totalProductAmount = 0;
        var productSupplyings = new List<ProductSupplying>();

        foreach (var product in products)
        {
            var unitRelation = product.UnitRelations.FirstOrDefault();
            if (unitRelation == null)
            {
                logger.Warning($"Sản phẩm {product.Name} chưa có đơn vị tính.");
                continue;
            }

            int quantity = 10;
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
                    ExpiryDate = DateTimeOffset.UtcNow.AddMonths(12),
                }
            );
        }

        decimal totalEquipmentAmount = 0;
        var equipmentSupplyings = new List<EquipmentSupplying>();

        for (int i = 1; i <= 20; i++)
        {
            decimal price = 1500000 + i * 100000;

            totalEquipmentAmount += price;

            equipmentSupplyings.Add(
                new EquipmentSupplying
                {
                    Name = $"Máy {(i % 2 == 0 ? "Sấy" : "Giặt")} {i}",
                    Code = Generator.GenerateCode("EQ", 6),
                    Price = price,
                    Quantity = 1,
                    SupplierId = supplier.Id,
                }
            );
        }

        decimal totalAmount = totalProductAmount + totalEquipmentAmount;
        decimal paidAmount = totalAmount;

        var document = new InventoryDocument(
            code: Generator.GenerateCode("IM", 6),
            amount: totalAmount,
            type: InventoryType.Import,
            branchId: branchId,
            note: "Nhập hàng khởi tạo"
        );

        foreach (var p in productSupplyings)
        {
            document.ProductSupplyings.Add(p);
        }

        foreach (var e in equipmentSupplyings)
        {
            document.EquipmentSupplyings.Add(e);
        }

        foreach (var p in productSupplyings)
        {
            p.InventoryDocument = document;
        }
        foreach (var e in equipmentSupplyings)
        {
            e.InventoryDocument = document;
        }

        await unitOfWork.Repository<InventoryDocument>().AddAsync(document, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);

        // 👉 Update status và phát event
        document.UpdateStatus(InventoryStatus.Completed);
        await unitOfWork.SaveAsync(cancellationToken);

        if (paidAmount > 0)
        {
            var invoice = new InventoryInvoice
            {
                Amount = paidAmount,
                Status = ActivationStatus.Active,
                SupplierId = supplier.Id,
                TransactionAt = DateTimeOffset.UtcNow,
            };

            var relation = new InventoryRelation
            {
                Amount = paidAmount,
                InventoryDocument = document,
                InventoryInvoice = invoice,
            };

            invoice.InventoryRelationships.Add(relation);

            await unitOfWork.Repository<InventoryInvoice>().AddAsync(invoice, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
        }
    }

    private static async Task InitVouchersAsync(
        IUnitOfWork unitOfWork,
        IQrGenerator barcode,
        CancellationToken cancellationToken
    )
    {
        var random = new Random();
        var customerResult = await unitOfWork
            .Repository<User>()
            .QueryAsync(x => x.Role == "CUSTOMER")
            .Select(SelectOnlyId())
            .ToListAsync();
        var vouchers = new List<Voucher>
        {
            new Voucher(
                code: "GIATUI10",
                title: "Giảm Giá Chào Mừng",
                imgUrl: null,
                barcode: barcode.GenerateQrBase64("FIXED20K"),
                discountFixed: false,
                discountValue: 10.0m,
                startAt: DateTimeOffset.UtcNow,
                endAt: DateTimeOffset.UtcNow.AddMonths(1),
                status: ActivationStatus.Active,
                description: "Giảm 10% cho khách hàng mới sử dụng dịch vụ giặt ủi"
            )
            {
                VoucherCustomers = customerResult
                    .Select(x => new VoucherCustomer { CustomerId = x.Id, IsUsed = false })
                    .ToList(),
            },
            new Voucher(
                code: "HAPPYBIRTHDAY",
                title: "Giảm Giá Sinh Nhật",
                imgUrl: null,
                barcode: barcode.GenerateQrBase64("FIXED20K"),
                discountFixed: false,
                discountValue: 10.0m,
                startAt: DateTimeOffset.UtcNow,
                endAt: DateTimeOffset.UtcNow.AddDays(1),
                status: ActivationStatus.Active,
                description: "Giảm 10% cho khách hàng mới sử dụng dịch vụ giặt ủi"
            ),
            new Voucher(
                code: "SUMMER15",
                title: "Ưu Đãi Mùa Hè",
                imgUrl: null,
                barcode: barcode.GenerateQrBase64("FIXED20K"),
                discountFixed: false,
                discountValue: 15.0m,
                startAt: DateTimeOffset.UtcNow,
                endAt: DateTimeOffset.UtcNow.AddMonths(2),
                status: ActivationStatus.Active,
                description: "Giảm 15% cho tất cả dịch vụ giặt ủi trong mùa hè"
            )
            {
                VoucherCustomers = customerResult
                    .Select(x => new VoucherCustomer { CustomerId = x.Id, IsUsed = false })
                    .ToList(),
            },
            new Voucher(
                code: "FIXED20K",
                title: "Giảm Giá Cố Định",
                imgUrl: null,
                barcode: barcode.GenerateQrBase64("FIXED20K"),
                discountFixed: true,
                discountValue: 20000.0m,
                startAt: DateTimeOffset.UtcNow,
                endAt: DateTimeOffset.UtcNow.AddMonths(3),
                status: ActivationStatus.Active,
                description: "Giảm 20.000 VNĐ cho hóa đơn giặt ủi tiếp theo"
            )
            {
                VoucherCustomers = customerResult
                    .Select(x => new VoucherCustomer { CustomerId = x.Id, IsUsed = false })
                    .ToList(),
            },
        };

        await unitOfWork.Repository<Voucher>().AddRangeAsync(vouchers, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
    }
}
