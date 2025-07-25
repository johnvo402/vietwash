using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Vouchers;
using Application.Feature.Vouchers.Commands.Update;
using Contracts.Common.Messages;
using Domain.Aggregates.Vouchers;
using FluentValidation;

namespace Application.Feature.Vouchers.Command.Update
{
    public class UpdateVoucherCommandValidator : AbstractValidator<UpdateVoucherCommand>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;

        public UpdateVoucherCommandValidator(
            IUnitOfWork unitOfWork,
            IActionAccessorService accessorService
        )
        {
            this.unitOfWork = unitOfWork;
            this.accessorService = accessorService;
            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.Voucher)
                .SetValidator(new VoucherValidator(unitOfWork, accessorService));

        }
        private async Task<bool> IsVoucherExistsAsync(long equipmentId, CancellationToken cancellation)
        {
            return await unitOfWork.Repository<Voucher>().AnyAsync(s => s.Id == equipmentId, cancellation);
        }
    }
}
