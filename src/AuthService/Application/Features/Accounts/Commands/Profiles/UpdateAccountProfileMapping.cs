using Contracts.Extensions;
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

        if (model.AccountContacts?.Any() == true)
        {
            entity.AccountContacts = model.AccountContacts.ToListMapping(x => new AccountContact
            {
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                Commune = x.Commune,
                District = x.District,
                Province = x.Province,
                CommuneCode = x.CommuneCode,
                DistrictCode = x.DistrictCode,
                ProvinceCode = x.ProvinceCode,
                Street = x.Street,
            });
        }

        return entity;
    }
}
