using Application.Features.Common.Projections.Branches.Branch;
using Domain.Aggregates.Branches;

namespace Application.Features.Branches.Commands.Update
{
    public static class BranchMapper
    {
        public static void MapToEntity(this Branch entity, BranchModel model)
        {
            entity.Update(
                model.Name,
                model.Code,
                model.Main,
                model.Status,
                model.Email,
                model.PhoneCode,
                model.PhoneNumber,
                model.AddressName,
                model.CommuneName,
                model.CommuneCode,
                model.DistrictName,
                model.DistrictCode,
                model.ProvinceName,
                model.ProvinceCode,
                model.Street
            );
        }
    }
}
