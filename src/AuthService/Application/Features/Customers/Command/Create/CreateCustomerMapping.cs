using Domain.Aggregates.Accounts;
using Infrastructure.Constants;

namespace Application.Features.Customers.Command.Create
{
	public static class CreateCustomerMapping
	{
		public static Account ToAccount(this CreateCustomerCommand command, string code)
		{
			return new Account(
				displayName: string.IsNullOrWhiteSpace(command.DisplayName) ? command.PhoneNumber! : command.DisplayName,
				phoneNumber: command.PhoneNumber!,
				password: null,
				email: null,
				role: ROLE.CUSTOMER,
				code: code
			);
		}
	}
}
