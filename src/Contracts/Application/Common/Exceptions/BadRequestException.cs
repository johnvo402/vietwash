using Contracts.ApiWrapper;
using JohnChum.SharedKernel.Domain.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Exceptions;

public class BadRequestException(IEnumerable<MessageResult> errors)
    : CustomException("One or several errors have occured")
{
    public virtual int HttpStatusCode { get; protected set; } = StatusCodes.Status400BadRequest;

    public IEnumerable<BadRequestError> Errors { get; set; } =
        errors.Select(x => new BadRequestError
        {
            Reasons =
            [
                new()
                {
                    Message = x.Message,
                    En = x.En,
                    Vi = x.Vi,
                },
            ],
        });
}
