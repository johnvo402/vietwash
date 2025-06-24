using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Mapping.Units;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Mediator;
using Unit = Domain.Aggregates.Services.Unit;

namespace Application.Feature.Units.Command.Update
{
    public class UpdateUnitHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateUnitCommand, Result<UpdateUnitResponse>>
    {
        public async ValueTask<Result<UpdateUnitResponse>> Handle(
            UpdateUnitCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var unit = await unitOfWork
                    .Repository<Unit>()
                    .FindByIdAsync(request.UnitId, cancellationToken);

                if (unit == null)
                {
                    return Result<UpdateUnitResponse>.Failure(
                        new NotFoundError(
                            "Unit not found",
                            Messager
                                .Create<Unit>()
                                .Message(MessageType.Found)
                                .Negative()
                                .BuildMessage()
                        )
                    );
                }
                unit.FromUpdateUnit(request.Unit);
                // Bắt đầu transaction
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

                // Cập nhật và lưu thay đổi
                await unitOfWork.Repository<Unit>().UpdateAsync(unit);
                await unitOfWork.SaveAsync(cancellationToken);

                // Commit transaction
                await unitOfWork.CommitAsync(cancellationToken);

                // Trả về phản hồi với thông tin cập nhật
                return Result<UpdateUnitResponse>.Success(
                    new UpdateUnitResponse
                    {
                        Message = "Unit updated successfully",
                        Name = unit.Name,
                    }
                );
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
