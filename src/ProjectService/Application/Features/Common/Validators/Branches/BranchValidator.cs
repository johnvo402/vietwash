using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.Branches.Branch;
using Domain.Aggregates.Branches;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;

namespace Application.Features.Common.Validators.Branches
{
    public partial class BranchValidator : AbstractValidator<BranchModel>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService _actionAccessorService;

        public BranchValidator(IUnitOfWork _unitOfWork, IActionAccessorService actionAccessorService)
        {
            this.unitOfWork = _unitOfWork;
            this._actionAccessorService = actionAccessorService;
            ApplyRules();
            _actionAccessorService = actionAccessorService;
        }
        private void ApplyRules()
        {
            RuleFor(b => b.Name)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.Name)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MaximumLength(256)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.Name)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(b => b.Code)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.Code)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MaximumLength(50)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.Code)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(b => b.Email)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.Email)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .Must(x =>
                {
                    Regex regex = EmailValidationRegex();
                    return regex.IsMatch(x!);
                })
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.Email)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build()
                )
                .MustAsync(
                    async (email, cancellationToken) => !await unitOfWork.Repository<Branch>().AnyAsync( e => e.Email == email, cancellationToken )
                )
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.Email)
                        .Message(MessageType.Existence)
                        .Build()
                );

            RuleFor(b => b.PhoneNumber)
                .MaximumLength(20)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.PhoneNumber)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(b => b.PhoneCode)
                .MaximumLength(10)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.PhoneCode)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(b => b.AddressName)
                .MaximumLength(256)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.AddressName)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(b => b.Street)
                .MaximumLength(256)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.Street)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(b => b.ProvinceName)
                .MaximumLength(128)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.ProvinceName)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(b => b.ProvinceCode)
                .MaximumLength(20)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.ProvinceCode)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(b => b.DistrictName)
                .MaximumLength(128)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.DistrictName)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(b => b.DistrictCode)
                .MaximumLength(20)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.DistrictCode)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(b => b.CommuneName)
                .MaximumLength(128)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.CommuneName)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(b => b.CommuneCode)
                .MaximumLength(20)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.CommuneCode)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(b => b.Slug)
                .MaximumLength(128)
                .WithState(x =>
                    Messager
                        .Create<Branch>()
                        .Property(x => x.Slug)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

        }

        [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
        private static partial Regex EmailValidationRegex();
    }
}
