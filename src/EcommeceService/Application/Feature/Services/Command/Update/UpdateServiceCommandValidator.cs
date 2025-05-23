using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Services;
using FluentValidation;
using Infrastructure.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;

namespace Application.Feature.Services.Command.Update
{
    public class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;

        public UpdateServiceCommandValidator(
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
			RuleFor(x => x.ServiceId)
				.NotEmpty()
				.WithState(x =>
						Messager
							.Create<Service>()
							.Property(x => x.CategoryId)
							.Message(MessageType.Null)
							.Negative()
							.Build()
				)
				.MustAsync(async (id, cancellation) =>
				{
					return await unitOfWork.Repository<Service>().AnyAsync(s => s.Id == id, cancellation);
				}).WithMessage("Service does not exist.");
			RuleFor(x => x.Service.Name)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Service>()
                        .Property(x => x.Name)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MaximumLength(256)
                .WithState(x =>
                    Messager
                        .Create<Service>()
                        .Property(x => x.Name)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );
			RuleFor(x => x.Service.CategoryId)
			    .NotEmpty()
			    .WithState(x =>
					    Messager
						    .Create<Service>()
						    .Property(x => x.CategoryId)
						    .Message(MessageType.Null)
						    .Negative()
						    .Build()
			    )
			    .MustAsync(async (categoryId, cancellation) =>
			    {
				    var categoryExists = await unitOfWork.Repository<Category>().AnyAsync(c => c.Id == categoryId, cancellation);
				    return categoryExists;
			    })
			    .WithState(_ =>
					    Messager
						    .Create<Service>()
						    .Property(x => x.CategoryId)
						    .Message(MessageType.Found)
						    .Negative()
						    .Build()
			    );
		}
    }
}
