using Application.Common.Security;

namespace Application.Feature.InventoryDocuments.Queries.GetReceipt
{
    public class InventoryReceiptResponse
    {
        [File]
        public string Url { get; set; } = default!;
    }
}
