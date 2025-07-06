using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.Accounts;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using FluentValidation;
using Infrastructure.UnitOfWorks;

namespace Application.Features.Common.Validators.Accounts;

public partial class AccountValidator : AbstractValidator<AccountModel>
{
    private readonly IActionAccessorService accessorService;
    private readonly IUnitOfWork unitOfWork;

    public AccountValidator(IActionAccessorService accessorService, IUnitOfWork unitOfWork)

    {
        this.accessorService = accessorService;
        this.unitOfWork = unitOfWork;
        ApplyRules();
    }

    private void ApplyRules()
    {
        _ = long.TryParse(accessorService.Id, out long id);

        RuleFor(x => x.DisplayName)
            .Cascade(CascadeMode.Stop) //Dừng khi NotEmpty() fail
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<Account>()
                    .Property(x => x.DisplayName)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .MaximumLength(256)
            .WithState(x =>
                Messager
                    .Create<Account>()
                    .Property(x => x.DisplayName)
                    .Message(MessageType.MaximumLength)
                    .Build()
            );

        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop) //Dừng khi NotEmpty() fail
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
            )
            .MustAsync(async (phone, cancellation) => !await IsPhoneExists(phone, cancellation))
            .WithState(x =>
                Messager
                    .Create<Account>()
                    .Property(x => x.PhoneNumber)
                    .Message(MessageType.Existence)
                    .Negative()
                    .Build()
            );
    }

    [GeneratedRegex(@"^\+?\d{7,15}$")]
    private static partial Regex PhoneValidationRegex();
    private async Task<bool> IsPhoneExists(string phoneNumber, CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<Account>()
            .AnyAsync(p => p.PhoneNumber == phoneNumber, cancellationToken);
    }
}
