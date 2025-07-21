using Application.Common.Interfaces.Services;
using Application.Features.Common.Validators.Accounts;
using Contracts.Common.Messages;
using FluentValidation;

namespace Application.Features.Customers.Command.Update;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
	public UpdateCustomerCommandValidator(IActionAccessorService accessorService)
	{
		_ = long.TryParse(accessorService.Id, out long id);

		RuleFor(x => x.Account)
			.NotEmpty()
			.WithState(x =>
				Messager
					.Create<UpdateCustomerCommand>()
					.Property(x => x.Account!)
					.Message(MessageType.Null)
					.Negative()
					.Build()
			)
			.SetValidator(new AccountValidator(accessorService)!);
	}
}
