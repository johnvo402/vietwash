using Application.Features.Common.Projections.Roles;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Roles;
using FluentValidation;

namespace Application.Features.Common.Validators.Roles;

public class RolePermissionValidator : AbstractValidator<RolePermissionModel>
{
    public RolePermissionValidator()
    {
        RuleFor(x => x.PermissionId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<RolePermission>(nameof(Role.RolePermissions))
                    .Property(x => x.PermissionId!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );
    }
}
