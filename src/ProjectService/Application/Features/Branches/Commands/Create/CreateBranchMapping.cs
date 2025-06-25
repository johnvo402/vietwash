using Application.Features.Common.Projections.Branches.Branch;
using Domain.Aggregates.Branches;

namespace Application.Features.Branches.Commands.Create
{
    public static class CreateBranchMapping
    {
        public static Branch ToEntity(this BranchModel model)
        {
            return new Branch(
                name: model.Name,
                code: model.Code,
                main: model.Main,
                status: model.Status,
                email: model.Email,
                phoneCode: model.PhoneCode,
                phoneNumber: model.PhoneNumber,
                addressName: model.AddressName,
                communeName: model.CommuneName,
                communeCode: model.CommuneCode,
                districtName: model.DistrictName,
                districtCode: model.DistrictCode,
                provinceName: model.ProvinceName,
                provinceCode: model.ProvinceCode,
                street: model.Street
            );
        }
    }
}
