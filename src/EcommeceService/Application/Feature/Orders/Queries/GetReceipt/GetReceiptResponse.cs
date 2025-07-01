using Application.Common.Security;

namespace Application.Feature.Orders.Queries.GetReceipt
{
    public class GetReceiptResponse
    {
        [File]
        public string ReceiptUrl { get; set; }
    }
}
