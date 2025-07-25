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
        return customer;
    }
}
