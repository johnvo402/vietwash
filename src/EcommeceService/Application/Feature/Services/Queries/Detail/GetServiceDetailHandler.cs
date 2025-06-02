using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Feature.Services.Queries.Detail;
public class GetServiceDetailHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetServiceDetailQuery, GetServiceDetailResponse>
{
    public async ValueTask<GetServiceDetailResponse> Handle(
        GetServiceDetailQuery command,
        CancellationToken cancellationToken
    )
    {

        var service =
            await unitOfWork
                .Repository<Service>()
                .FindByConditionAsync(
                    new GetServiceWithIncludeByIdSpecification(command.ServiceId),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<Service>().Message(MessageType.Found).Negative().BuildMessage()]
            );


        var response = mapper.Map<GetServiceDetailResponse>(service);

   

        return response;
    }
}
