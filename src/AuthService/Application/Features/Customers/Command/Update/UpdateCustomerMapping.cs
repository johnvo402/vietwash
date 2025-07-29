using Domain.Aggregates.Accounts;
using Infrastructure.Constants;

namespace Application.Features.Customers.Command.Update;

public static class UpdateCustomerMapping
{
    public static Account FromUpdateCustomer(this Account customer, UpdateCustomerModel update)
    {
        customer.Update(
            update.DisplayName,
            update.PhoneNumber,
            status: update.Status,
            role: ROLE.CUSTOMER,
            gender: update.Gender
        );
        if (update.AccountContact != null)
            customer.AccountContact = new AccountContact
            {
                PhoneNumber = update.AccountContact.PhoneNumber,
                Address = update.AccountContact.Address,
                Commune = update.AccountContact.Commune,
                District = update.AccountContact.District,
                Province = update.AccountContact.Province,
                CommuneCode = update.AccountContact.CommuneCode,
                DistrictCode = update.AccountContact.DistrictCode,
                ProvinceCode = update.AccountContact.ProvinceCode,
                Street = update.AccountContact.Street,
            };

        return customer;
    }
}
