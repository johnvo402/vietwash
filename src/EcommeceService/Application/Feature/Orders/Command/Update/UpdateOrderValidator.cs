using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Orders;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Services;
using FluentValidation;

namespace Application.Feature.Orders.Command.Update
{
    public class UpdateOrderValidator : AbstractValidator<UpdateOrderCommand>
    {
        public UpdateOrderValidator(IUnitOfWork _unitOfWork)
        {
            RuleFor(x => x.Model.Note)
                .MaximumLength(500)
                .WithState(x =>
                    Messager
                        .Create<UpdateOrderModel>(nameof(Order))
                        .Property(x => x.Note)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(x => x.Model.OrderItems)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<UpdateOrderModel>(nameof(Order))
                        .Property(x => x.OrderItems)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                );

            RuleForEach(x => x.Model.OrderItems)
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
