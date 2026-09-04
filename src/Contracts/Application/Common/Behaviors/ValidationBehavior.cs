using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Behaviors;

public sealed class ValidationBehavior<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> validators
) : MessagePreProcessor<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    protected override async ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TMessage>(message);

            // Validators share the request's identity and DbContext. EF contexts
            // cannot run simultaneous queries, so evaluate them sequentially.
            List<ValidationResult> validationResults = [];
            foreach (var validator in validators)
                validationResults.Add(await validator.ValidateAsync(context, cancellationToken));

            List<ValidationFailure> failures = validationResults
                .Where(r => !r.IsValid)
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                // Ném ValidationException với danh sách ValidationFailure
                throw new ValidationException(failures);
            }
        }

        return;
    }
}
