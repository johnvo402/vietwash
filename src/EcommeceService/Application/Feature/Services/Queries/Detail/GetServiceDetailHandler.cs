using System.Threading.Tasks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
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

        //User? createdByUser = null;
        //User? updatedByUser = null;
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

  //      if (service.CreatedBy != null)
  //      {
  //          createdByUser = await unitOfWork
  //             .Repository<User>()
  //             .FindByConditionAsync(
  //                 new GetUserByIdWithoutIncludeSpecification(long.Parse(service.CreatedBy)),
  //                 cancellationToken
  //             );
		//}
  //      if (service.UpdatedBy != null)
  //      {
  //          updatedByUser = !string.IsNullOrEmpty(service.UpdatedBy)
  //         ? await unitOfWork
  //             .Repository<User>()
  //             .FindByConditionAsync(
  //                 new GetUserByIdWithoutIncludeSpecification(long.Parse(service.UpdatedBy)),
  //                 cancellationToken
  //             )
  //         : null;
  //      }

        var response = mapper.Map<GetServiceDetailResponse>(service);

        //if (createdByUser != null)
        //    response.CreatedByUser = createdByUser != null ? mapper.Map<UserDTO>(createdByUser) : null;
        //if (updatedByUser != null)
        //    response.UpdatedByUser = updatedByUser != null ? mapper.Map<UserDTO>(updatedByUser) : null;

        return response;
    }
}
