using Application.Common.Security;

namespace Application.Features.EInvoices.Queries.GetByCode
{
    public class GetEInvoiceByCodeResponse
    {
        [File]
        public string Url { get; set; } = default!;
    }
}
