using Application.Common.Security;

namespace Application.Features.EInvoices.Queries.GetByOrderId
{
    public class GetEInvoiceByOrderIdResponse
    {
        [File]
        public string Url { get; set; } = default!;
    }
}
