using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Equipments;
using Application.Feature.Common.Validators.Vouchers;
using Application.Feature.Equipments.Command.Create;
using FluentValidation;
using System.Linq;

namespace Application.Feature.Vouchers.Commands.Create
{
    public class CreateVoucherCommandValidator : AbstractValidator<CreateVoucherCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IActionAccessorService _accessorService;

        public CreateVoucherCommandValidator(
            IUnitOfWork unitOfWork,
            IActionAccessorService accessorService
        )
        {
            _unitOfWork = unitOfWork;
            _accessorService = accessorService;
            ApplyRules();
        }

        private void ApplyRules()
        {
            Include(new VoucherValidator(_unitOfWork, _accessorService));
        }
    }
}
