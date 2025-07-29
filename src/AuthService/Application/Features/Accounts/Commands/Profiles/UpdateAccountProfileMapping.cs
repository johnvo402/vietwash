using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Commands.Profiles;

public static class UpdateAccountProfileMapping
{
    public static Account FromUpdateModel(this Account entity, UpdateAccountProfileCommand model)
    {
        entity.Update(
            displayName: model.DisplayName,
            email: model.Email,
            phoneNumber: model.PhoneNumber,
            birthDay: model.BirthDay != null
                ? DateOnly.FromDateTime((DateTime)model.BirthDay)
                : null,
            gender: model.Gender
        );

        if (!string.IsNullOrWhiteSpace(model.AvtUrl))
            entity.AvtUrl = model.AvtUrl;

        entity.AccountContact =
            model.AccountContact != null
                ? new AccountContact
                {
                    PhoneNumber = model.AccountContact.PhoneNumber,
                    Address = model.AccountContact.Address,
                    Commune = model.AccountContact.Commune,
                    District = model.AccountContact.District,
                    Province = model.AccountContact.Province,
                    CommuneCode = model.AccountContact.CommuneCode,
                    DistrictCode = model.AccountContact.DistrictCode,
                    ProvinceCode = model.AccountContact.ProvinceCode,
                    Street = model.AccountContact.Street,
                }
                : null;

        return entity;
    }
}
