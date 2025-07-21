using Application.Common.Interfaces.Services;
using Application.Features.Customers.Command.Create;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Application.Features.Customers.Command
{
	public partial class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
	{
		private readonly IActionAccessorService accessorService;

		public CreateCustomerCommandValidator(IActionAccessorService accessorService)
		{
			this.accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			_ = long.TryParse(accessorService.Id, out long id);

			RuleFor(x => x.DisplayName)
				.MaximumLength(256)
				.WithState(x =>
					Messager
						.Create<Account>()
						.Property(x => x.DisplayName)
						.Message(MessageType.MaximumLength)
						.Build()
				);

			RuleFor(x => x.PhoneNumber)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<Account>()
						.Property(x => x.PhoneNumber)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				)
				.Must(x =>
				{
					Regex regex = PhoneValidationRegex();
					return regex.IsMatch(x!);
				})
				.WithState(x =>
					Messager
						.Create<Account>()
						.Property(x => x.PhoneNumber)
						.Message(MessageType.Valid)
						.Negative()
						.Build()
				);
		}

		[GeneratedRegex(@"^\+?\d{7,15}$")]
		private static partial Regex PhoneValidationRegex();
	}

}
