using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Vouchers;
using Contracts.Common.Messages;
using Domain.Aggregates.Vouchers;
using FluentValidation;

namespace Application.Feature.Common.Validators.Vouchers
{
    public class VoucherValidator : AbstractValidator<VoucherModel>
    {
        public VoucherValidator(IUnitOfWork _, IActionAccessorService __)
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Voucher>()
                        .Property(x => x.Title)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.ImgUrl)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Voucher>()
                        .Property(x => x.ImgUrl)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.DiscountValue)
                .GreaterThanOrEqualTo(0)
                .WithState(x =>
                    Messager
                        .Create<Voucher>()
                        .Property(x => x.DiscountValue)
                        .Message(MessageType.GreaterThanEqual)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100)
                .When(x => !x.DiscountFixed)
                .WithState(x =>
                    Messager
                        .Create<Voucher>()
                        .Property(x => x.DiscountValue)
                        .Message(MessageType.LessThanEqual)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.EndAt)
                .GreaterThan(x => x.StartAt)
                .WithState(x =>
                    Messager
                        .Create<Voucher>()
                        .Property(x => x.EndAt)
                        .Message(MessageType.Expired)
                        .Negative()
                        .Build()
                );
        }
    }
}
