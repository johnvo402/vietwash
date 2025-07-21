
using Domain.Aggregates.Accounts.Enums;

namespace Application.Features.Common.Projections.Customers;

public class CustomerModel
{
	public string? DisplayName { get; set; }
	public string? PhoneNumber { get; set; }
	public Gender? Gender { get; set; }
}
