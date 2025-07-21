using Application.Feature.Common.Validators.Equipments;
using FluentValidation;

namespace Application.Feature.Equipments.Command.Update
{
    public class UpdateEquipmentCommandValidator : AbstractValidator<UpdateEquipmentCommand>
    {
        public UpdateEquipmentCommandValidator()
        {
            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.Equipment).SetValidator(new EquipmentUpdateValidator());
        }
    }
}
