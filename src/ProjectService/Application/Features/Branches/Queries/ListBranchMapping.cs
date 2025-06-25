using System.Linq.Expressions;
using Application.Features.Common.Projections.Branches.Branch;
using Domain.Aggregates.Branches;

namespace Application.Features.Branches.Queries
{
    public static class ListBranchMapping
    {
        public static Expression<Func<Branch, ListBranchResponse>> Selector() =>
            entity => new ListBranchResponse
            {
                Id = entity.Id,
                PublicId = entity.PublicId,
                Name = entity.Name,
                Code = entity.Code,
                Main = entity.Main,
                Status = entity.Status,
                Email = entity.Email,
                PhoneCode = entity.PhoneCode,
                PhoneNumber = entity.PhoneNumber,
                AddressName = entity.AddressName,
                CommuneName = entity.CommuneName,
                CommuneCode = entity.CommuneCode,
                DistrictName = entity.DistrictName,
                DistrictCode = entity.DistrictCode,
                ProvinceName = entity.ProvinceName,
                ProvinceCode = entity.ProvinceCode,
                Street = entity.Street,
                Slug = entity.Slug,
            };
    }
}
