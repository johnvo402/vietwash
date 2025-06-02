using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.Users;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Features.Common.Validators.Users;

public partial class UserValidator : AbstractValidator<UserModel>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IActionAccessorService accessorService;

    public UserValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
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
                    .Create<User>()
                    .Property(x => x.DisplayName)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .MaximumLength(256)
            .WithState(x =>
            Messager
                    .Create<User>()
                    .Property(x => x.DisplayName)
                    .Message(MessageType.MaximumLength)
                    .Build()
            );

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithState(x =>
            Messager
                    .Create<User>()
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
                    .Create<User>()
                    .Property(x => x.PhoneNumber)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build()
            );
    }

    [GeneratedRegex(@"^\+?\d{7,15}$")]
    private static partial Regex PhoneValidationRegex();
}
