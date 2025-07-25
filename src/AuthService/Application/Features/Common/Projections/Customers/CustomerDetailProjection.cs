using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;

namespace Application.Features.Common.Projections.Customers
{
	public class CustomerDetailProjection : CustomerProjection
	{
		public Gender Gender { get; set; }
		public string? Email { get; set; }
		public DateOnly BirthDay { get; set; }

		public override void MappingFrom(Account account)
		{
			base.MappingFrom(account);
			BirthDay = account.BirthDay;
			Gender = account.Gender ?? Gender.Other;
			Email = account.Email;
		}
	}
}
