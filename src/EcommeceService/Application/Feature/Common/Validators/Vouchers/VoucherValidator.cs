using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Equipments;
using Application.Feature.Common.Projections.Vouchers;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Vouchers;
using FluentValidation;

namespace Application.Feature.Common.Validators.Vouchers
{
    public class VoucherValidator
        : AbstractValidator<VoucherModel>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;

        public VoucherValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
        {
            this.unitOfWork = unitOfWork;
            this.accessorService = accessorService;
            ApplyRules();
        }

        private void ApplyRules()
        {


            RuleFor(x => x.Title)
                .NotEmpty()
                .WithState(x => Messager
                    .Create<Voucher>()
                    .Property(x => x.Title)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build());

            RuleFor(x => x.Barcode)
                .NotEmpty()
                .WithState(x => Messager
                    .Create<Voucher>()
                    .Property(x => x.Barcode)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build());

            RuleFor(x => x.ImgUrl)
                .NotEmpty()
                .WithState(x => Messager
                    .Create<Voucher>()
                    .Property(x => x.ImgUrl)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build());

            RuleFor(x => x.DiscountValue)
                .GreaterThan(0)
                .WithState(x => Messager
                    .Create<Voucher>()
                    .Property(x => x.DiscountValue)
                    .Message(MessageType.LessThanEqual)
                    .Negative()
                    .Build());

            RuleFor(x => x.EndAt)
                .GreaterThan(x => x.StartAt)
                .WithState(x => Messager
                    .Create<Voucher>()
                    .Property(x => x.EndAt)
                    .Message(MessageType.Expired)
                    .Negative()
                    .Build());

        }


    }
}
