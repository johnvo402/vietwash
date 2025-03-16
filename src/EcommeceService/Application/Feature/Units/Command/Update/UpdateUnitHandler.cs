using Application.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Units;
using AutoMapper;
using Domain.Aggregates.Services;
using FluentValidation.Results;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unit = Domain.Aggregates.Services.Unit;

namespace Application.Feature.Units.Command.Update
{
	public class UpdateUnitHandler(
		IUnitOfWork unitOfWork,
		IMapper mapper
	) : IRequestHandler<UpdateUnitCommand, UpdateUnitResponse>
	{
		public async ValueTask<UpdateUnitResponse> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
		{
			try
			{
				// Chuyển đổi UnitId thành Ulid
				if (!Ulid.TryParse(request.UnitId, out var parsedUnitId))
				{
					throw new ValidationException(
						new List<ValidationFailure>
						{
							new("UnitId", "Unit ID must be a valid Ulid.")
						}
					);
				}

				// Tìm Unit theo ID (sử dụng Ulid)
				var unit = await unitOfWork.Repository<Unit>()
					.FindByIdAsync(parsedUnitId, cancellationToken);

				if (unit == null)
				{
					throw new NotFoundException(
						[Messager.Create<Unit>().Message(MessageType.Found).Negative().BuildMessage()]
					);
				}

				// Ánh xạ dữ liệu từ command vào unit (chỉ Name)
				mapper.Map(request.Unit, unit);
				// Bắt đầu transaction
				using var transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

				// Cập nhật và lưu thay đổi
				await unitOfWork.Repository<Unit>().UpdateAsync(unit);
				await unitOfWork.SaveAsync(cancellationToken);

				// Commit transaction
				await transaction.CommitAsync(cancellationToken);

				// Trả về phản hồi với thông tin cập nhật
				return new UpdateUnitResponse
				{
					Message = "Unit updated successfully",
					Name = unit.Name
				};
			}
			catch (Exception ex)
			{
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
