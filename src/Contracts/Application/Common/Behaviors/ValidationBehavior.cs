using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Behaviors;

public sealed class ValidationBehavior<TMessage, TResponse>(
    IServiceScopeFactory serviceScopeFactory
) : MessagePreProcessor<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    protected override async ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IServiceProvider serviceProvider = scope.ServiceProvider;
        IEnumerable<IValidator<TMessage>> validators = serviceProvider.GetRequiredService<
            IEnumerable<IValidator<TMessage>>
        >();

        if (validators.Any())
        {
            var context = new ValidationContext<TMessage>(message);

            IEnumerable<ValidationResult> validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

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
