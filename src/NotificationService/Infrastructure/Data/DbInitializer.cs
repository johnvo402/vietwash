using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shared.Kernel.Extensions;

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
            if (!(await unitOfWork.Repository<NotificationTemplate>().AnyAsync()))
            {
                await InitializeNotificationTemplatesAsync(unitOfWork, cancellationToken);
            }
            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.Error("Lỗi xảy ra trong khi khởi tạo dữ liệu: {Message}", ex.Message);
            throw;
        }
    }

    private static async Task InitializeNotificationTemplatesAsync(
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken
    )
    {
        var templates = new List<NotificationTemplate>
        {
            new NotificationTemplate
            {
                Id = "inventory_import",
                Title = SerializerExtension
                    .Serialize(
                        new
                        {
                            vi = "Phiếu nhập đã được tạo",
                            en = "Inventor import has been created",
                        }
                    )
                    .StringJson,
                Content = SerializerExtension
                    .Serialize(
                        new
                        {
                            vi = "Phiếu nhập {{code}} vào chi nhánh {{branch_name}} đã được tạo lúc {{time}}.",
                            en = "Inventor import {{code}} for branch {{branch_name}} has been created at {{time}}.",
                        }
                    )
                    .StringJson,
                ContentHtml = SerializerExtension
                    .Serialize(
                        new
                        {
                            vi = "Phiếu nhập <strong>{{code}}</strong> vào chi nhánh <strong>{{branch_name}}</strong> đã được tạo lúc <strong>{{time}}</strong>.",
                            en = "Inventor import <strong>{{code}}</strong> for branch <strong>{{branch_name}}</strong> has been created at <strong>{{time}}</strong>.",
                        }
                    )
                    .StringJson,
            },
            new NotificationTemplate
            {
                Id = "inventory_export",
                Title = SerializerExtension
                    .Serialize(
                        new
                        {
                            vi = "Phiếu xuất đã được tạo",
                            en = "Inventory export has been created",
                        }
                    )
                    .StringJson,
                Content = SerializerExtension
                    .Serialize(
                        new
                        {
                            vi = "Phiếu xuất {{code}} từ chi nhánh {{branch_name}} đã được tạo lúc {{time}}.",
                            en = "Inventory export {{code}} from branch {{branch_name}} has been created at {{time}}.",
                        }
                    )
                    .StringJson,
                ContentHtml = SerializerExtension
                    .Serialize(
                        new
                        {
                            vi = "Phiếu xuất <strong>{{code}}</strong> từ chi nhánh <strong>{{branch_name}}</strong> đã được tạo lúc <strong>{{time}}</strong>.",
                            en = "Inventory export <strong>{{code}}</strong> from branch <strong>{{branch_name}}</strong> has been created at <strong>{{time}}</strong>.",
                        }
                    )
                    .StringJson,
            },
        };

        foreach (var template in templates)
        {
            await unitOfWork
                .Repository<NotificationTemplate>()
                .AddAsync(template, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
        }
    }
}
