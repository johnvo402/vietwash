using Application.Common.Interfaces.Services.Elastics;
using Domain.Aggregates.AuditLogs;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.UseCases.AuditLogs.Queries;

public class ListAuditlogHandler(IElasticsearchServiceFactory? elasticsearch = null)
    : IRequestHandler<ListAuditlogQuery, PaginationResponse<ListAuditlogResponse>>
{
    public async ValueTask<PaginationResponse<ListAuditlogResponse>> Handle(
        ListAuditlogQuery request,
        CancellationToken cancellationToken
    )
    {
        if (elasticsearch == null)
        {
            throw new NotImplementedException();
        }
        return await elasticsearch
            .Get<AuditLog>()
            .PaginatedListAsync<ListAuditlogResponse>(request);
    }
}
