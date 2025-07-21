using Application.Features.Accounts.Commands.Update;
using Domain.Aggregates.Accounts;
using Infrastructure.Constants;

namespace Application.Features.Customers.Command.Update;

public static class UpdateCustomerMapping
{
	public static Account FromUpdateCustomer(this Account customer, UpdateCustomerModel update)
	{
		customer.Update(
			update.DisplayName,
			update.Email,
			update.PhoneNumber,
			update.BirthDay != null ? DateOnly.FromDateTime((DateTime)update.BirthDay) : null,
			status: update.Status,
			role: ROLE.CUSTOMER,
			gender: update.Gender
		);
		customer.AvtUrl = update.AvtUrl;
		return customer;
	}
}
