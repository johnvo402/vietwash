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
            await InitializeNotificationTemplatesAsync(unitOfWork, cancellationToken);

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
                            vi = "Phiếu nhập {{code}} vào chi nhánh {{branch_name}} đã được tạo.",
                            en = "Inventor import {{code}} for branch {{branch_name}}.",
                        }
                    )
                    .StringJson,
                ContentHtml = SerializerExtension
                    .Serialize(
                        new
                        {
                            vi = "Phiếu nhập <strong id=\"import_id\">{{code}}</strong> vào chi nhánh <strong>{{branch_name}}</strong> đã được tạo.",
                            en = "Inventor import <strong id=\"import_id\">{{code}}</strong> for branch <strong>{{branch_name}}</strong>.",
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
                            vi = "Phiếu xuất {{code}} từ chi nhánh {{branch_name}} đã được tạo.",
                            en = "Inventory export {{code}} from branch {{branch_name}}.",
                        }
                    )
                    .StringJson,
                ContentHtml = SerializerExtension
                    .Serialize(
                        new
                        {
                            vi = "Phiếu xuất <strong id=\"export_id\">{{code}}</strong> từ chi nhánh <strong>{{branch_name}}</strong> đã được tạo.",
                            en = "Inventory export <strong id=\"export_id\">{{code}}</strong> from branch <strong>{{branch_name}}</strong>.",
                        }
                    )
                    .StringJson,
            },
            new NotificationTemplate
            {
                Id = "laundry_processed",
                Title = SerializerExtension
                    .Serialize(
                        new { vi = "Đồ giặt đã xử lý xong", en = "Laundry has been processed" }
                    )
                    .StringJson,
                Content = SerializerExtension
                    .Serialize(
                        new
                        {
                            vi = "Đơn giặt {{order_code}} của bạn tại chi nhánh {{branch_name}} đã được xử lý xong.",
                            en = "Your laundry order {{order_code}} at branch {{branch_name}} has been processed.",
                        }
                    )
                    .StringJson,
                ContentHtml = SerializerExtension
                    .Serialize(
                        new
                        {
                            vi = "Đơn giặt <strong id=\"order_id\">{{order_code}}</strong> của bạn tại chi nhánh <strong>{{branch_name}}</strong> đã được xử lý xong.",
                            en = "Your laundry order <strong id=\"order_id\">{{order_code}}</strong> at branch <strong>{{branch_name}}</strong> has been processed.",
                        }
                    )
                    .StringJson,
            },
            new NotificationTemplate
            {
                Id = "happy_birthday",
                Title = SerializerExtension
                    .Serialize(new { vi = "Chúc mừng sinh nhật!", en = "Happy Birthday!" })
                    .StringJson,
                Content = SerializerExtension
                    .Serialize(
                        new
                        {
                            vi = "Chúc {{customer_name}} có một ngày sinh nhật thật vui vẻ và ý nghĩa! VietWash xin tặng bạn một voucher sinh nhật sử dụng đến hết ngày {{voucher_expiry}}.",
                            en = "Wishing {{customer_name}} a joyful and meaningful birthday! VietWash would like to give you a birthday voucher valid until {{voucher_expiry}}",
                        }
                    )
                    .StringJson,
                ContentHtml = SerializerExtension
                    .Serialize(
                        new
                        {
                            vi = "🎉 Chúc <strong>{{customer_name}}</strong> có một ngày sinh nhật thật <strong>vui vẻ</strong> và <strong>ý nghĩa</strong>!<br/>🎁 VietWash xin tặng bạn một <strong>voucher sinh nhật{{voucher_value}}</strong> sử dụng đến hết ngày <strong>{{voucher_expiry}}</strong>.",
                            en = "🎉 Wishing <strong>{{customer_name}}</strong> a <strong>joyful</strong> and <strong>meaningful</strong> birthday!<br/>🎁 VietWash would like to give you a <strong>birthday voucher{{voucher_value}}</strong> valid until the end of <strong>{{voucher_expiry}}</strong>.",
                        }
                    )
                    .StringJson,
            },
        };

        var repository = unitOfWork.Repository<NotificationTemplate>();

        foreach (var template in templates)
        {
            var existing = await repository.FindByIdAsync(template.Id, cancellationToken);
            if (existing is null)
            {
                await repository.AddAsync(template, cancellationToken);
            }
            else
            {
                existing.Title = template.Title;
                existing.Content = template.Content;
                existing.ContentHtml = template.ContentHtml;
                await repository.UpdateAsync(existing);
            }
        }

        await unitOfWork.SaveAsync(cancellationToken);
    }
}
