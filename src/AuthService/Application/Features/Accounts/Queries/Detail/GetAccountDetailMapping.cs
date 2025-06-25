using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Queries.Detail;

public static class GetAccountDetailMapping
{
    public static GetAccountDetailResponse ToGetAccountDetailResponse(this Account account)
    {
        var response = new GetAccountDetailResponse();
        response.MappingFrom(account);

        return response;
    }
}
