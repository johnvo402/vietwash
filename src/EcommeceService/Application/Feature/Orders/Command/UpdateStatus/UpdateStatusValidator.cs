using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using FluentValidation;

namespace Application.Feature.Orders.Command.UpdateStatus
{
    public class UpdateStatusValidator : AbstractValidator<UpdateStatusCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _accessorService;

        public UpdateStatusValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
        {
            _unitOfWork = unitOfWork;
            _accessorService = accessorService;
            ApplyRules();
        }

        private void ApplyRules()
        {
            _ = long.TryParse(_accessorService.Id, out long id);

            RuleFor(x => x.Status)
                .Must(newStatus => newStatus != OrderStatus.Completed)
                .WithState(_ =>
                    Messager
                        .Create<UpdateStatusCommand>(nameof(Order))
                        .Property(x => x.Status)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.Status)
                .MustAsync(
                    async (command, newStatus, _) => await CheckStatusCompleted(id, newStatus)
                )
                .WithState(_ =>
                    Messager
                        .Create<UpdateStatusCommand>(nameof(Order))
                        .Property(x => x.Status)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build()
                );
        }

        private async Task<bool> CheckStatusCompleted(long id, OrderStatus? newStatus)
        {
            if (newStatus != OrderStatus.Cancelled)
                return true;

            // Kiểm tra xem đơn hàng hiện tại có phải Completed không
            var isCompleted = await _unitOfWork
                .Repository<Order>()
                .AnyAsync(x => x.Id == id && x.Status == OrderStatus.Completed);

            // Nếu đơn hàng đang Completed => không cho đổi sang Cancelled
            return !isCompleted;
        }
    }
}
