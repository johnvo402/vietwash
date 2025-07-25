using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.EInvoices;
using Domain.Aggregates.EInvoices.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EInvoices.Queries.GetByOrderId
{
    public class GetEInvoiceByOrderIdHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetEInvoiceByOrderIdQuery, Result<GetEInvoiceByOrderIdResponse>>
    {
        public async ValueTask<Result<GetEInvoiceByOrderIdResponse>> Handle(
            GetEInvoiceByOrderIdQuery request,
            CancellationToken cancellationToken
        )
        {
            var url = await unitOfWork
                .Repository<EInvoice>()
                .QueryAsync(x =>
                    x.OrderId == request.OrderId && x.Status == EInvoiceStatus.Published
                )
                .Select(x => x.PdfUrl)
                .FirstOrDefaultAsync();

            if (url == null)
            {
                return Result<GetEInvoiceByOrderIdResponse>.Failure(
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
            return Result<GetEInvoiceByOrderIdResponse>.Success(
                new GetEInvoiceByOrderIdResponse { Url = url }
            );
        }
    }
}
