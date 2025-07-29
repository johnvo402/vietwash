using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Suppliers.Query.ImportExportHistory
{
    public class ImportExportHistoryQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ImportExportHistoryResponse>>>;
}
