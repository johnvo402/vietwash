using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.Accounts;
using Domain.Aggregates.Accounts;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.Validators.Accounts;

public partial class AccountValidator : AbstractValidator<AccountModel>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IActionAccessorService accessorService;

    public AccountValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
    {
        this.unitOfWork = unitOfWork;
        this.accessorService = accessorService;
        ApplyRules();
    }

    private void ApplyRules()
    {
        _ = long.TryParse(accessorService.Id, out long id);

        RuleFor(x => x.DisplayName)
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
