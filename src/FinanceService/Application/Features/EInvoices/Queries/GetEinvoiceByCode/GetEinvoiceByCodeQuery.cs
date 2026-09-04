using Contracts.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.EInvoices.Queries.GetByCode
{
    public class GetEInvoiceByCodeQuery : IRequest<Result<GetEInvoiceByCodeResponse>>
    {
        [FromRoute(Name = "Code")]
        public string Code { get; set; } = default!;
    }
}
