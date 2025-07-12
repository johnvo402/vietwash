using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Feedbacks;
using Ardalis.GuardClauses;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using FluentValidation;
using Wangkanai;

namespace Application.Feature.Common.Validators.Feedbacks
{
    public class FeedbackValidator : AbstractValidator<FeedbackModel>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentAccount currentCustomer;
        IActionAccessorService _accessorService;

        public FeedbackValidator(
            IUnitOfWork unitOfWork,
            ICurrentAccount currentCustomer,
            IActionAccessorService accessorService
        )
        {
            _accessorService = accessorService;
            this.unitOfWork = unitOfWork;
            this.currentCustomer = currentCustomer;
            ApplyRules();
        }

        private void ApplyRules()
        {
            _ = long.TryParse(_accessorService.Id, out long id);
            RuleFor(x => currentCustomer.Id)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Feedback>()
                        .Property(x => x.User)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                );
            RuleFor(x => x.Comment)
                .MaximumLength(500)
                .WithState(x =>
                    Messager
                        .Create<FeedbackModel>(nameof(Feedback))
                        .Property(x => x.Comment)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );
            RuleFor(x => x.Rating)
                .GreaterThan(0)
                .WithState(x =>
                    Messager
                        .Create<FeedbackModel>(nameof(Feedback))
                        .Property(x => x.Rating)
                        .Message(MessageType.GreaterThan)
                        .Negative()
                        .Build()
                )
                .LessThanOrEqualTo(5)
                .WithState(x =>
                    Messager
                        .Create<FeedbackModel>(nameof(Feedback))
                        .Property(x => x.Rating)
                        .Message(MessageType.LessThanEqual)
                        .Negative()
                        .Build()
                );

            RuleFor(x => currentCustomer.Id)
                .MustAsync(
                    (model, ct) =>
                    {
                        if (currentCustomer.Id is null)
                            return Task.FromResult(false);
                        return IsCustomerUseServiceAsync((long)currentCustomer.Id, id, ct);
                    }
                )
                .WithState(x =>
                    Messager
                        .Create<Feedback>()
                        .Property(x => x.User)
                        .Message(MessageType.Existence)
                        .Negative()
                        .Build()
                );
        }

        private async Task<bool> IsCustomerUseServiceAsync(
            long customerId,
            long serviceId,
            CancellationToken cancellationToken
        )
        {
            return await unitOfWork
                .Repository<Order>()
                .AnyAsync(
                    order =>
                        order.CustomerId == customerId
                        && order.OrderItems.Any(item => item.ServiceId == serviceId)
                        && order.Status == OrderStatus.Completed,
                    cancellationToken
                );
        }
    }
}
