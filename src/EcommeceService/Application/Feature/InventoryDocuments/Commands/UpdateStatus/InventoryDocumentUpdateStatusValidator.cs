using Contracts.Common.Messages;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using FluentValidation;

namespace Application.Feature.InventoryDocuments.Commands.UpdateStatus
{
    public class InventoryDocumentUpdateStatusValidator
        : AbstractValidator<InventoryDocumentUpdateStatusCommand>
    {
        public InventoryDocumentUpdateStatusValidator()
        {
            RuleFor(x => x.ModelStatus.Status)
                .IsInEnum()
                .WithState(x =>
                    Messager
                        .Create<InventoryDocumentUpdateStatusCommand>(nameof(InventoryDocument))
                        .Property(x => x.ModelStatus.Status)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build()
                );

            When(
                x => x.ModelStatus.Status == InventoryStatus.Canceled,
                () =>
                {
                    RuleFor(x => x.ModelStatus.CancelReason)
                        .NotEmpty()
                        .WithState(x =>
                            Messager
                                .Create<InventoryDocumentUpdateStatusCommand>(
                                    nameof(InventoryDocument)
                                )
                                .Property(x => x.ModelStatus.CancelReason)
                                .Message(MessageType.Null)
                                .Negative()
                                .Build()
                        );
                }
            );
        }
    }
}
