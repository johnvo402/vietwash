using Domain.Aggregates.Inventories.Enums;
using JohnChum.SharedKernel.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.InventoryImports
{
    public class InventoryImportProjection : BaseResponse<long>
    {
        public long BranchId { get; set; }
        public long ToWarehouseId { get; set; }
        public long SupplierId { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
        public InventoryPaymentMethod PaymentMethod { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateTimeOffset TransactionAt { get; set; }
        public string Code { get; set; } = string.Empty;
        public InventoryDocumentStatus Status { get; set; }
        public InventoryDocumentType Type { get; set; }
    }
}
