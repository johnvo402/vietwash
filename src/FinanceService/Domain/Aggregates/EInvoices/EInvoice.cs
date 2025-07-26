using Ardalis.GuardClauses;
using Domain.Aggregates.EInvoices.Enums;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.EInvoices
{
    public class EInvoice : AggregateRoot
    {
        public string InvoiceSymbol { get; set; } = default!;

        public long InvoiceNumber { get; set; }
        public long OrderId { get; set; }
        public DateTime OrderDate { get; set; }

        public string LookupCode { get; set; } = null!;

        public string OrgName { get; set; } = null!;

        public string OrgTaxCode { get; set; } = null!;

        public string OrgAddress { get; set; } = null!;

        public string OrgPhone { get; set; } = null!;

        public string? OrgLogo { get; set; }

        public string? OrgStamp { get; set; }

        public string CustomerName { get; set; } = null!;

        public string? CustomerEmail { get; set; }

        public string? CustomerPhone { get; set; }
        public string? CustomerTaxCode { get; set; }
        public decimal Total { get; set; }

        public int VatPercent { get; set; }

        public decimal TaxTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TotalWithTax { get; set; }

        public string? QrCodeUrl { get; set; }

        public string? PdfUrl { get; set; }

        public EInvoiceStatus Status { get; set; }

        public List<EInvoiceItem> Items { get; set; } = new();

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            switch (domainEvent)
            {
                default:
                    return false;
            }
        }

        public EInvoice() { }

        public EInvoice(
            long orderId,
            string invoiceSymbol,
            DateTime orderDate,
            string lookupCode,
            string orgName,
            string orgTaxCode,
            string orgAddress,
            string orgPhone,
            string customerName,
            decimal total,
            int vatPercent,
            decimal taxTotal,
            decimal totalWithTax,
            List<EInvoiceItem> items,
            decimal discount = 0,
            string? customerEmail = null,
            string? customerPhone = null,
            string? qrCodeUrl = null,
            string? orgLogo = null,
            string? orgStamp = null
        )
        {
            Guard.Against.NullOrWhiteSpace(invoiceSymbol);
            Guard.Against.NullOrWhiteSpace(lookupCode);
            Guard.Against.NullOrWhiteSpace(orgName);
            Guard.Against.NullOrWhiteSpace(orgTaxCode);
            Guard.Against.NullOrWhiteSpace(orgAddress);
            Guard.Against.NullOrWhiteSpace(orgPhone);
            Guard.Against.NullOrWhiteSpace(customerName);
            Guard.Against.Negative(total);
            Guard.Against.OutOfRange(vatPercent, nameof(vatPercent), 0, 100);
            Guard.Against.Negative(taxTotal);
            Guard.Against.Negative(totalWithTax);
            Guard.Against.NullOrEmpty(items);
            Guard.Against.NegativeOrZero(orderId);

            OrderId = orderId;
            InvoiceSymbol = invoiceSymbol;
            OrderDate = orderDate;
            LookupCode = lookupCode;
            OrgName = orgName;
            OrgTaxCode = orgTaxCode;
            OrgAddress = orgAddress;
            OrgPhone = orgPhone;
            OrgLogo = orgLogo;
            OrgStamp = orgStamp;
            CustomerName = customerName;
            CustomerEmail = customerEmail;
            CustomerPhone = customerPhone;
            Total = total;
            VatPercent = vatPercent;
            TaxTotal = taxTotal;
            TotalWithTax = totalWithTax;
            QrCodeUrl = qrCodeUrl;
            Items = items;
            Discount = discount;
            Status = EInvoiceStatus.Pending;
        }

        // ✅ Update method
        public void Update(
            long? orderId = null,
            string? invoiceSymbol = null,
            DateTime? orderDate = null,
            string? lookupCode = null,
            string? orgName = null,
            string? orgTaxCode = null,
            string? orgAddress = null,
            string? orgPhone = null,
            string? customerName = null,
            string? customerEmail = null,
            string? customerPhone = null,
            decimal? total = null,
            int? vatPercent = null,
            decimal? taxTotal = null,
            decimal? totalWithTax = null,
            string? qrCodeUrl = null,
            string? orgLogo = null,
            string? orgStamp = null
        )
        {
            if (!string.IsNullOrWhiteSpace(invoiceSymbol))
                InvoiceSymbol = invoiceSymbol;
            if (orderId.HasValue)
                OrderId = orderId.Value;

            if (orderDate.HasValue)
                OrderDate = orderDate.Value;
            if (!string.IsNullOrWhiteSpace(lookupCode))
                LookupCode = lookupCode;
            if (!string.IsNullOrWhiteSpace(orgName))
                OrgName = orgName;
            if (!string.IsNullOrWhiteSpace(orgTaxCode))
                OrgTaxCode = orgTaxCode;
            if (!string.IsNullOrWhiteSpace(orgAddress))
                OrgAddress = orgAddress;
            if (!string.IsNullOrWhiteSpace(orgPhone))
                OrgPhone = orgPhone;
            if (!string.IsNullOrWhiteSpace(customerName))
                CustomerName = customerName;
            if (!string.IsNullOrWhiteSpace(customerEmail))
                CustomerEmail = customerEmail;
            if (!string.IsNullOrWhiteSpace(customerPhone))
                CustomerPhone = customerPhone;
            if (total.HasValue)
                Total = total.Value;
            if (vatPercent.HasValue)
                VatPercent = vatPercent.Value;
            if (taxTotal.HasValue)
                TaxTotal = taxTotal.Value;
            if (totalWithTax.HasValue)
                TotalWithTax = totalWithTax.Value;
            if (!string.IsNullOrWhiteSpace(qrCodeUrl))
                QrCodeUrl = qrCodeUrl;
            if (!string.IsNullOrWhiteSpace(orgLogo))
                OrgLogo = orgLogo;
            if (!string.IsNullOrWhiteSpace(orgStamp))
                OrgStamp = orgStamp;
        }

        // ✅ Replace items
        public void ReplaceItems(List<EInvoiceItem> newItems)
        {
            Guard.Against.NullOrEmpty(newItems);
            Items = newItems;
        }
    }
}
