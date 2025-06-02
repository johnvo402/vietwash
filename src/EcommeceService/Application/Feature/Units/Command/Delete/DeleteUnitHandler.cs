using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using Unit = Domain.Aggregates.Services.Unit;


namespace Application.Feature.Units.Command.Delete
{
	public class DeleteUnitHandler(IUnitOfWork unitOfWork)
	: IRequestHandler<DeleteUnitCommand>
	{ 
		public async ValueTask<Mediator.Unit> Handle(DeleteUnitCommand command, CancellationToken cancellationToken)
		{
			var unit = await unitOfWork.Repository<Unit>()
				.FindByConditionAsync(
					new GetUnitByIdWithoutIncludeSpecification(command.UnitId),
					cancellationToken
				)
			?? throw new NotFoundException(
				[Messager.Create<Unit>().Message(MessageType.Found).Negative().BuildMessage()]
			);

			await unitOfWork.Repository<Unit>().DeleteAsync(unit);
			await unitOfWork.SaveAsync(cancellationToken);

			return Mediator.Unit.Value;
		}
	}
}
