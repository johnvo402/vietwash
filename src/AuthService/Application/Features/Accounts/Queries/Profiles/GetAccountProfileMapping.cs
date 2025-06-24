using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Queries.Profiles;

public static class GetAccountProfileMapping
{
    public static GetAccountProfileResponse ToGetAccountProfileResponse(this Account user)
    {
        var response = new GetAccountProfileResponse();
        response.MappingFrom(user);

        return response;
    }
}
