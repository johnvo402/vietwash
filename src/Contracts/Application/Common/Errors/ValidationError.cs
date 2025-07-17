using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class ValidationError(List<ValidationFailure> invalidParams)
    : ErrorDetails(
        "The request parameters didn't validate.",
        [
            .. invalidParams
                .GroupBy(x => x.PropertyName)
                .Select(failureGroups => new InvalidParam
                {
                    PropertyName = failureGroups.Key,
                    Reasons = failureGroups.Select(failure =>
                    {
                        if (failure.CustomState is MessageResult messageResult)
                        {
                            return new ErrorReason
                            {
                                Message = messageResult.Message ?? "Invalid value",
                                En = messageResult.En ?? "Invalid value",
                                Vi = messageResult.Vi ?? "Giá trị không hợp lệ",
                            };
                        }
                        return new ErrorReason
                        {
                            Message = failure.ErrorMessage ?? "Unknown error",
                            En = failure.ErrorMessage ?? "Unknown error",
                            Vi = failure.ErrorMessage ?? "Lỗi không xác định",
                        };
                    }),
                }),
        ],
        nameof(ValidationError),
        StatusCodes.Status400BadRequest
    );
