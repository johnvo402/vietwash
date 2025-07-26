using Application.Common.Models.EInvoices;
using Contracts.Infrastructure.Common;
using Contracts.Utils;
using Domain.Aggregates.EInvoices;

namespace Application.Events.CreateEInvoiceEvents
{
    public static class CreateEInvoiceEventMapping
    {
        public static EInvoice CreateFromMessage(
            this EInvoiceOrderMessage message,
            OrgSetting org,
            string invoiceSymbol,
            string lookupCode,
            string? qrCodeUrl = null
        )
        {
            var taxTotal = Math.Round(message.Total * org.VatPercent / 100, 2);
            var totalWithTax = message.Total + taxTotal;

            var items = message
                .Items.Select(x => new EInvoiceItem
                {
                    ServiceName = x.ServiceName,
                    UnitRelationName = x.UnitRelationName ?? "lần",
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    TotalPrice = x.TotalPrice,
                })
                .ToList();

            return new EInvoice(
                orderId: message.OrderId,
                invoiceSymbol: invoiceSymbol,
                orderDate: message.CompletedAt.DateTime,
                lookupCode: lookupCode,
                orgName: org.OrgName,
                orgTaxCode: org.OrgTaxCode,
                orgAddress: org.OrgAddress,
                orgPhone: org.OrgPhone,
                customerName: message.CustomerName,
                total: message.Total,
                vatPercent: org.VatPercent,
                taxTotal: taxTotal,
                totalWithTax: totalWithTax,
                items: items,
                customerEmail: message.CustomerEmail,
                customerPhone: message.CustomerPhone,
                qrCodeUrl: qrCodeUrl,
                orgLogo: org.Logo,
                orgStamp: org.Stamp,
                discount: message.Discount
            );
        }

        public static ReceiptModel MapToReceiptModel(
            this EInvoice einvoice,
            string logo,
            string stamp
        )
        {
            return new ReceiptModel
            {
                InvoiceSymbol = einvoice.InvoiceSymbol,
                InvoiceNumber = einvoice.InvoiceNumber.ToString("D10"),
                OrderDate = einvoice.OrderDate,
                LookupCode = einvoice.LookupCode,

                OrgInfo = new OrgInfoModel
                {
                    Name = einvoice.OrgName,
                    TaxCode = einvoice.OrgTaxCode,
                    Address = einvoice.OrgAddress,
                    Phone = einvoice.OrgPhone,
                    Logo = logo,
                    Stamp = stamp,
                },

                CustomerName = einvoice.CustomerName,
                CustomerEmail = einvoice.CustomerEmail,
                CustomerPhone = einvoice.CustomerPhone,
                CustomerTaxCode = einvoice.CustomerTaxCode,

                OrderItems = einvoice
                    .Items.Select(item => new ReceiptItemModel
                    {
                        ServiceName = item.ServiceName,
                        UnitRelationName = item.UnitRelationName ?? "",
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice.FormatCurrency(),
                        TotalPriceItem = item.TotalPrice.FormatCurrency(),
                    })
                    .ToList(),

                Total = einvoice.Total.FormatCurrency(),
                VatPercent = einvoice.VatPercent,
                TaxTotal = einvoice.TaxTotal.FormatCurrency(),
                Discount = "0", // Nếu có chiết khấu thì bổ sung từ dữ liệu khác
                TotalWithTax = einvoice.TotalWithTax.FormatCurrency(),
                TotalInWords = NumberToTextConverter.ToVietnameseCurrencyText(
                    einvoice.TotalWithTax
                ), // bạn cần viết hàm chuyển số thành chữ
                QrCodeUrl = einvoice.QrCodeUrl ?? "",
            };
        }
    }
}
