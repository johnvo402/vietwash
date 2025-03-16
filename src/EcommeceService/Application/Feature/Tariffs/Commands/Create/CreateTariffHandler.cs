using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Users.Commands.Create;
using AutoMapper;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Tariffs;
using Mediator;

namespace Application.Feature.Tariffs.Commands.Create
{
    public class CreateTariffHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediaUpdateService<Tariff> mediaUpdateService
) : IRequestHandler<CreateTariffCommand, QueueResponse<CreateTariffCommand>>
    {
        public async ValueTask<QueueResponse<CreateTariffCommand>> Handle(CreateTariffCommand request, CancellationToken cancellationToken)
        {
            Tariff mappingTaiff = mapper.Map<Tariff>(request.Payload);
            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

                Tariff tariff = await unitOfWork
                    .Repository<Tariff>()
                    .AddAsync(mappingTaiff, cancellationToken);

                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);
                return new QueueResponse<CreateTariffCommand>
                {
                    Error = "lỗi",
                    ErrorType = Contracts.Dtos.Responses.QueueErrorType.Transient,
                    IsSuccess = false,
                    ResponseData = request,
                    LastAttemptTime = DateTime.UtcNow,
                    PayloadId = request.PayloadId,
                };
            }
            catch (Exception ex)
            {

                await unitOfWork.RollbackAsync(cancellationToken);
                return new QueueResponse<CreateTariffCommand>
                {
                    Error = ex.Message,
                    ErrorType = Contracts.Dtos.Responses.QueueErrorType.Transient,
                    IsSuccess = false,
                    ResponseData = request,
                    LastAttemptTime = DateTime.UtcNow,
                    PayloadId = request.PayloadId,
                };
            }
        }
    }
}