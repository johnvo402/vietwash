using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Services.Command.Create;
using AutoMapper;
using Domain.Aggregates.Services;
using Mediator;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unit = Domain.Aggregates.Services.Unit;

namespace Application.Feature.Units.Command.Create
{
    public class CreateUnitHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper
    ) : IRequestHandler<CreateUnitCommand, CreateUnitResponse>
    {
        public async ValueTask<CreateUnitResponse> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
        {
            Unit mappingUnit = mapper.Map<Unit>(request);
            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

                Unit unit = await unitOfWork
                    .Repository<Unit>()
                    .AddAsync(mappingUnit, cancellationToken);

                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                return new CreateUnitResponse
                {
                    Message = "Unit created successfully",
                };
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
