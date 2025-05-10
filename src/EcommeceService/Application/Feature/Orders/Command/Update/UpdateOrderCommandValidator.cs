using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Orders;
using Application.Feature.Orders.Command.Create;
using Domain.Aggregates.Orders;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Command.Update
{
	public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IActionAccessorService _accessorService;

		public UpdateOrderCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
		{
			_unitOfWork = unitOfWork;
			_accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			//Include(new OrderValidator(_unitOfWork, _accessorService));

			// Rule cho OrderId
			RuleFor(x => x.OrderId)
				.NotEmpty()
				.WithState(x => Messager.Create<UpdateOrderCommand>(nameof(Order)).Property(x => x.OrderId).Message(MessageType.Null).Negative().Build())
				.Must(id => Ulid.TryParse(id, out _)) // Kiểm tra OrderId có phải Ulid hợp lệ không
				.WithState(x => Messager.Create<UpdateOrderCommand>(nameof(Order)).Property(x => x.OrderId).Message(MessageType.Valid).Negative().Build())
				.MustAsync(async (id, ct) => await _unitOfWork.Repository<Order>().AnyAsync(o => o.Id == long.Parse(id), ct))
				.WithState(x => Messager.Create<UpdateOrderCommand>(nameof(Order)).Property(x => x.OrderId).Message(MessageType.Existence).Negative().Build());

			// Rule cho Order (tái sử dụng OrderValidator)
			RuleFor(x => x.Order)
				.NotNull()
				.WithState(x => Messager.Create<UpdateOrderCommand>(nameof(Order)).Property(x => x.Order).Message(MessageType.Null).Negative().Build());
				//.SetValidator(new OrderValidator(_unitOfWork, _accessorService)); // Include OrderValidator
		}
	}
}
