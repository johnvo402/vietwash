using System.Linq.Expressions;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.GenIdLong;
using Contracts.Dtos.Requests;
using Contracts.Utils;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Enums;
using Domain.Aggregates.Services.Specifications;
using Domain.Aggregates.Suppliers;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Infrastructure.Constants;
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
        var logger = provider.GetRequiredService<ILogger>();
        var idGenerator = provider.GetRequiredService<IIdGenerator>();
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
            // Initialize Orders
            if (!await unitOfWork.Repository<Order>().AnyAsync())
            {
                logger.Information("Bắt đầu khởi tạo dữ liệu đơn hàng...");

                await InitializeOrdersAsync(unitOfWork, idGenerator, logger, cancellationToken);

                logger.Information("Hoàn tất khởi tạo dữ liệu đơn hàng...");
            }
            else
            {
                logger.Information("Dữ liệu đơn hàng đã tồn tại, bỏ qua khởi tạo.");
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
        IIdGenerator idGenerator, // Thêm vào đây
        ILogger logger,
        CancellationToken cancellationToken
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
                    Id = idGenerator.GenerateId(), // ✅ Thêm dòng này
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
            order.Id = idGenerator.GenerateId(); // ✅ Thêm dòng này
            order.PublicId = Ulid.NewUlid();
            foreach (var orderItem in orderItems)
            {
                order.OrderItems.Add(orderItem);
            }

            if (status == OrderStatus.Completed)
            {
                var paymentMethod = paymentMethods[random.Next(paymentMethods.Length)];
                var orderPayment = new OrderPayment
                {
                    Id = idGenerator.GenerateId(), // ✅ Thêm dòng này
                    Amount = order.Total,
                    PaymentMethod = paymentMethod,
                    PaymentDate = DateTimeOffset.UtcNow,
                };
                order.OrderPayments.Add(orderPayment);
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
        var branches = user.BranchUsers?.Select(bu => bu.BranchId).Distinct().ToList();
        var units = (await unitOfWork.Repository<Unit>().ListAsync(cancellationToken)).ToList();

        if (!categories.Any() || !(branches?.Any() ?? false) || !units.Any())
        {
            logger.Error(
                "Không thể khởi tạo dịch vụ: Thiếu dữ liệu danh mục, chi nhánh hoặc đơn vị tính."
            );
            throw new InvalidOperationException("Missing required Category, Branch, or Unit data.");
        }

        var random = new Random();
        var services = new List<Service>();

        for (int i = 1; i <= 20; i++)
        {
            // Randomly select Category and Branch
            var category = categories[random.Next(categories.Count)];
            var branch = branches[random.Next(branches.Count)];
            var unit = units[random.Next(units.Count)];
            var type = random.Next(2) == 0 ? TypeStatus.SingleService : TypeStatus.Combo;

            // Generate Service
            var service = new Service(
                categoryId: category.Id,
                branchId: branch,
                name: type == TypeStatus.SingleService ? $"Service {i}" : $"Combo {i}",
                type: type,
                status: ActivationStatus.Active,
                description: $"Description for service {i}",
                image: null
            )
            {
                Disable = false,
            };
            service.Slug = Generator.GenerateSlug(service.Name);
            // Add UnitRelation
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
        foreach (var service1 in services)
        {
            await unitOfWork.Repository<Service>().AddAsync(service1, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
        }
    }
}
