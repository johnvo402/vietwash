using System.Linq.Expressions;
using Application.Features.Common.Projections.Accounts;
using Domain.Aggregates.Accounts;

namespace Application.Features.Common.Mapping.Accounts;

public static class AccountMapping
{
    public static AccountProjection ToUserProjection(this Account account)
    {
        var response = new AccountProjection();
        response.MappingFrom(account);

        return response;
    }

    public static AccountContactProjection ToAccountContactProjectionResponse(
        this AccountContact contact
    )
    {
        return new()
        {
            Address = contact.Address,
            Commune = contact.Commune,
            District = contact.District,
            Province = contact.Province,
            CommuneCode = contact.CommuneCode,
            DistrictCode = contact.DistrictCode,
            ProvinceCode = contact.ProvinceCode,
            Street = contact.Street,
        };
    }

    public static BranchAccountProjection ToBranchAccountProjectionResponse(
        this BranchAccount branch
    )
    {
        return new()
        {
            AccountId = branch.AccountId,
            BranchId = branch.BranchId,
            BranchName = branch.BranchName,
        };
    }
}
