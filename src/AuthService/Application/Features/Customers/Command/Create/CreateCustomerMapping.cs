using System.Linq.Expressions;
using Application.Features.Common.Projections.Accounts;
using Domain.Aggregates.Accounts;
using Infrastructure.Constants;

namespace Application.Features.Customers.Command.Create
{
    public static class CreateCustomerMapping
    {
        public static Account ToAccount(
            this CreateCustomerCommand command,
            string code,
            IEnumerable<BranchAccountModel> branchAccounts
        )
        {
            return new Account(
                displayName: string.IsNullOrWhiteSpace(command.DisplayName)
                    ? command.PhoneNumber!
                    : command.DisplayName,
                phoneNumber: command.PhoneNumber!,
                password: null,
                email: null,
                role: ROLE.CUSTOMER,
                code: code
            )
            {
                Gender = command.Gender,
                BranchAccounts = branchAccounts
                    .Select(x => new BranchAccount
                    {
                        BranchId = x.BranchId,
                        BranchName = x.BranchName,
                    })
                    .ToList(),

                AccountContact =
                    command.AccountContact != null
                        ? new AccountContact
                        {
                            PhoneNumber = command.AccountContact.PhoneNumber,
                            Address = command.AccountContact.Address,
                            Commune = command.AccountContact.Commune,
                            District = command.AccountContact.District,
                            Province = command.AccountContact.Province,
                            CommuneCode = command.AccountContact.CommuneCode,
                            DistrictCode = command.AccountContact.DistrictCode,
                            ProvinceCode = command.AccountContact.ProvinceCode,
                            Street = command.AccountContact.Street,
                        }
                        : null,
            };
        }

        public static CreateCustomerResponse ToCreateCustomerResponse(this Account account)
        {
            return new CreateCustomerResponse
            {
                DisplayName = account.DisplayName,
                Id = account.Id,
                PhoneNumber = account.PhoneNumber,
            };
        }
    }
}
