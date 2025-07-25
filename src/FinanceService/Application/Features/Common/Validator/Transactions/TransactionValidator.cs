using Application.Features.Common.Projections.Transactions;
using Contracts.Common.Messages;
using Domain.Aggregates.Funds;
using FluentValidation;

namespace Application.Features.Common.Validators.Transactions
{
    public class TransactionValidator : AbstractValidator<TransactionModel>
    {
        public TransactionValidator()
        {
            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0)
                .WithState(x =>
                    Messager
                        .Create<Transaction>()
                        .Property(x => x.Customer)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithState(x =>
                    Messager
                        .Create<Transaction>()
                        .Property(x => x.Amount)
                        .Message(MessageType.GreaterThan)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.TransactionAt)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Transaction>()
                        .Property(x => x.TransactionAt)
                        .Message(MessageType.Empty)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithState(x =>
                    Messager
                        .Create<Transaction>()
                        .Property(x => x.Type)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build()
                );
        }
    }
}
