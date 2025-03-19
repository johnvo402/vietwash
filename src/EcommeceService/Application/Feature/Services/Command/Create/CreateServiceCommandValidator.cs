using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Services;
using FluentValidation;


namespace Application.Feature.Services.Command.Create;

public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IActionAccessorService _accessorService;

    public CreateServiceCommandValidator(
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
        Include(new ServiceValidator(_unitOfWork, _accessorService));
    }
}
