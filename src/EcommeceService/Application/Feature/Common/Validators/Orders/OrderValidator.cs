using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Orders;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Services;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Feature.Common.Validators.Orders
{
    public class OrderValidator : AbstractValidator<OrderModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _accessorService;

        public OrderValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
        {
            _unitOfWork = unitOfWork;
            _accessorService = accessorService;
            ApplyRules();
        }

        private void ApplyRules()
        {
            _ = long.TryParse(_accessorService.Id, out long id);

            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100)
                .When(x => x.DiscountFixed) // Giảm giá theo %
                .WithState(x =>
                    Messager
                        .Create<OrderModel>(nameof(Order))
                        .Property(x => x.DiscountValue)
                        .Message(MessageType.LessThanEqual)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.CustomerId)
                .NotNull()
                .WithState(x =>
                    Messager
                        .Create<OrderModel>(nameof(Order))
                        .Property(x => x.CustomerId)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MustAsync(
                    async (customerId, ct) =>
                        await _unitOfWork.Repository<User>().AnyAsync(u => u.Id == customerId, ct)
                )
                .WithState(x =>
                    Messager
                        .Create<OrderModel>(nameof(Order))
                        .Property(x => x.CustomerId)
                        .Message(MessageType.Found)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.DiscountValue)
                .GreaterThanOrEqualTo(0)
                .WithState(x =>
                    Messager
                        .Create<OrderModel>(nameof(Order))
                        .Property(x => x.DiscountValue)
                        .Message(MessageType.GreaterThanEqual)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.Note)
                .MaximumLength(500)
                .WithState(x =>
                    Messager
                        .Create<OrderModel>(nameof(Order))
                        .Property(x => x.Note)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(x => x.OrderItems)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<OrderModel>(nameof(Order))
                        .Property(x => x.OrderItems)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                );

            RuleForEach(x => x.OrderItems)
                .ChildRules(item =>
                {
                    item.RuleFor(x => x.ServiceId)
                        .NotEmpty()
                        .WithState(x =>
                            Messager
                                .Create<OrderItemModel>(nameof(OrderItem))
                                .Property(x => x.ServiceId)
                                .Message(MessageType.Null)
                                .Negative()
                                .Build()
                        )
                        .MustAsync(
                            async (serviceId, ct) =>
                                await _unitOfWork
                                    .Repository<Service>()
                                    .AnyAsync(s => s.Id == serviceId, ct)
                        )
                        .WithState(x =>
                            Messager
                                .Create<OrderItemModel>(nameof(OrderItem))
                                .Property(x => x.ServiceId)
                                .Message(MessageType.Existence)
                                .Negative()
                                .Build()
                        );

                    item.RuleFor(x => x.Price)
                        .GreaterThan(0)
                        .WithState(x =>
                            Messager
                                .Create<OrderItemModel>(nameof(OrderItem))
                                .Property(x => x.Price)
                                .Message(MessageType.GreaterThan)
                                .Negative()
                                .Build()
                        );
                });
        }
    }
}
