using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Feature.Orders.Queries.TotalOrderByStaff
{
    public class TotalOrderByStaffValidator : AbstractValidator<TotalOrderByStaffQuery>
    {
        private readonly IUnitOfWork _unitOfWork;

        public TotalOrderByStaffValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            ApplyRule();
        }

        private void ApplyRule()
        {
            RuleFor(x => x.StaffId)
                .MustAsync(CheckStaffExists)
                .WithState(x =>
                    Messager
                        .Create<TotalOrderByStaffQuery>(nameof(User))
                        .Property(x => x.StaffId)
                        .Message(MessageType.Existence)
                        .Negative()
                        .Build()
                );
        }

        private async Task<bool> CheckStaffExists(long id, CancellationToken cancellationToken)
        {
            return await _unitOfWork
                .Repository<User>()
                .AnyAsync(
                    x => x.Id == id && x.Status == ActivationStatus.Active,
                    cancellationToken
                );
        }
    }
}
