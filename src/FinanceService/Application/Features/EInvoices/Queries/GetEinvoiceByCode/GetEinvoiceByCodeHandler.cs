using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.EInvoices;
using Domain.Aggregates.EInvoices.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EInvoices.Queries.GetByCode
{
    public class GetEInvoiceByCodeHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetEInvoiceByCodeQuery, Result<GetEInvoiceByCodeResponse>>
    {
        public async ValueTask<Result<GetEInvoiceByCodeResponse>> Handle(
            GetEInvoiceByCodeQuery request,
            CancellationToken cancellationToken
        )
        {
            var url = await unitOfWork
                .Repository<EInvoice>()
                .QueryAsync(x =>
                    x.LookupCode == request.Code && x.Status == EInvoiceStatus.Published
                )
                .Select(x => x.PdfUrl)
                .FirstOrDefaultAsync();

            if (url == null)
            {
                return Result<GetEInvoiceByCodeResponse>.Failure(
                    new NotFoundError(
                        "The source not found",
                        Messager
                            .Create<EInvoice>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }
            return Result<GetEInvoiceByCodeResponse>.Success(
                new GetEInvoiceByCodeResponse { Url = url }
            );
        }
    }
}
