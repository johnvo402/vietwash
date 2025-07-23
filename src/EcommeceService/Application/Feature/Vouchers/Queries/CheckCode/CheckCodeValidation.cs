using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users;
using Domain.Aggregates.Vouchers;
using FluentValidation;

namespace Application.Feature.Vouchers.Queries.CheckCode
{
    public class CheckCodeValidation : AbstractValidator<CheckCodeQuery>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckCodeValidation(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            ApplyRule();
        }

        private void ApplyRule()
        {
            RuleFor(x => x.CustomerId)
                .MustAsync(CheckUserExists)
                .WithState(x =>
                    Messager.Create<User>().Message(MessageType.Existence).Negative().Build()
                );
            RuleFor(x => x.VoucherCode)
                .MustAsync(
                    (command, value, cts) =>
                        CheckVoucherCode(value, command.CustomerId, cancellation: cts)
                )
                .WithState(x =>
                    Messager.Create<Voucher>().Message(MessageType.Valid).Negative().Build()
                );
            RuleFor(x => x.VoucherCode)
                .MustAsync(
                    (command, value, cts) =>
                        CheckVoucherDate(value, command.CustomerId, cancellation: cts)
                )
                .WithState(x =>
                    Messager.Create<Voucher>().Message(MessageType.Expired).Negative().Build()
                );
        }

        private async Task<bool> CheckVoucherCode(
            string code,
            long customerId,
            CancellationToken cancellation
        )
        {
            return await _unitOfWork
                .Repository<Voucher>()
                .AnyAsync(
                    x =>
                        x.Code == code
                        && x.VoucherCustomers.Any(x => !x.IsUsed && x.CustomerId == customerId),
                    cancellation
                );
        }

        private async Task<bool> CheckVoucherDate(
            string code,
            long customerId,
            CancellationToken cancellation
        )
        {
            var now = DateTimeOffset.UtcNow;
            return await _unitOfWork
                .Repository<Voucher>()
                .AnyAsync(
                    x =>
                        x.Code == code
                        && x.StartAt <= now
                        && x.EndAt >= now
                        && x.VoucherCustomers.Any(x => !x.IsUsed && x.CustomerId == customerId),
                    cancellation
                );
        }

        private async Task<bool> CheckUserExists(long userId, CancellationToken cancellation)
        {
            var now = DateTimeOffset.UtcNow;
            return await _unitOfWork
                .Repository<User>()
                .AnyAsync(x => x.Id == userId && x.Status == ActivationStatus.Active, cancellation);
        }
    }
}
