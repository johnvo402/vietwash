using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Orders;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Services;
using Domain.Aggregates.Users;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Validators.Orders
{
	public class OrderValidator : AbstractValidator<CreateOrderModel>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IActionAccessorService _accessorService;

		public OrderValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
		{
			_unitOfWork = unitOfWork;
			_accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			_ = Ulid.TryParse(_accessorService.Id, out Ulid id);

			RuleFor(x => x.DiscountValue)
				.LessThanOrEqualTo(100)
				.When(x => x.DiscountType == true)// Giảm giá theo %
				.WithState(x => Messager
					.Create<CreateOrderModel>(nameof(Order))
					.Property(x => x.DiscountValue)
					.Message(MessageType.LessThanEqual)
					.Negative()
					.Build());
			RuleFor(x => x.CustomerId)
				//Customer không rỗng
				.NotEmpty()
				.WithState(x => Messager
					.Create<CreateOrderModel>(nameof(Order))
					.Property(x => x.CustomerId)
					.Message(MessageType.Null)
					.Negative()
					.Build())
				//Customer phải tồn tại
				.MustAsync(async (customerId, ct) =>
				{
					bool isValidUlid = Ulid.TryParse(customerId, out Ulid ulid);
					Console.WriteLine($"Parsed ULID: {ulid} (Valid: {isValidUlid})");

					if (!isValidUlid) return false;

					bool userExists = await _unitOfWork.Repository<User>().AnyAsync(u => u.Id.Equals(ulid), ct);
					Console.WriteLine($"User exists: {userExists}");

					return userExists;
				}).WithState(x => Messager
                    .Create<CreateOrderModel>(nameof(Order))
                    .Property(x => x.CustomerId)
                    .Message(MessageType.Found)
                    .Negative()
                    .Build());
			
			RuleFor(x => x.PaymentMethod)
				.IsInEnum()
				.WithState(x => Messager.Create<CreateOrderModel>(nameof(Order))
				.Property(x => x.PaymentMethod)
				.Message(MessageType.OuttaOption)
				.Build());

			RuleFor(x => x.DiscountValue)
				.GreaterThanOrEqualTo(0)
				.WithState(x => Messager
					.Create<CreateOrderModel>(nameof(Order))
					.Property(x => x.DiscountValue)
					.Message(MessageType.GreaterThanEqual)
					.Negative()
					.Build());

			RuleFor(x => x.Note)
				.MaximumLength(500)
				.WithState(x => Messager
					.Create<CreateOrderModel>(nameof(Order))
					.Property(x => x.Note)
					.Message(MessageType.MaximumLength)
					.Build());

			RuleFor(x => x.OrderItems)
				.NotEmpty()
				.WithState(x => Messager
					.Create<CreateOrderModel>(nameof(Order))
					.Property(x => x.OrderItems)
					.Message(MessageType.Null)
					.Negative()
					.Build());

			RuleForEach(x => x.OrderItems).ChildRules(item =>
			{
				item.RuleFor(x => x.ServiceId)
						.NotEmpty()
						.WithState(x => Messager
							.Create<CreateOrderItemModel>(nameof(OrderItem))
							.Property(x => x.ServiceId)
							.Message(MessageType.Null)
							.Negative()
							.Build())
						.MustAsync(async (serviceId, ct) =>
							await _unitOfWork.Repository<Service>().AnyAsync(s => s.Id.Equals(serviceId), ct))
						.WithState(x => Messager
							.Create<CreateOrderItemModel>(nameof(OrderItem))
							.Property(x => x.ServiceId)
							.Message(MessageType.Existence)
							.Negative()
							.Build());

				//item.RuleFor(x => x.UnitRelationId)
				//	.NotEmpty()
				//	.WithState(x => Messager
				//		.Create<OrderItemModel>(nameof(OrderItem))
				//		.Property(x => x.UnitRelationId)
				//		.Message(MessageType.Null)
				//		.Negative()
				//		.Build())
				//	.MustAsync(async (unitRelationId, ct) =>
				//		await _unitOfWork.Repository<UnitRelation>().AnyAsync(u => u.Id.Equals(Ulid.Parse(unitRelationId)), ct))
				//	.WithState(x => Messager
				//		.Create<OrderItemModel>(nameof(OrderItem))
				//		.Property(x => x.UnitRelationId)
				//		.Message(MessageType.Existence)
				//		.Negative()
				//		.Build());

				item.RuleFor(x => x.Price)
					.GreaterThan(0)
					.WithState(x => Messager
						.Create<CreateOrderItemModel>(nameof(OrderItem))
						.Property(x => x.Price)
						.Message(MessageType.GreaterThan)
						.Negative()
						.Build());
			});

			RuleFor(x => x.PaymentAmount)
				.GreaterThanOrEqualTo(0)
				.WithState(x => Messager
					.Create<CreateOrderModel>(nameof(Order))
					.Property(x => x.PaymentAmount)
					.Message(MessageType.GreaterThanEqual)
					.Negative()
					.Build());

			RuleFor(x => x)
				.MustAsync(async (model, ct) => await IsTotalValidAsync(model, ct)) // Sửa tham số thành OrderModel
				.WithState(x => Messager
					.Create<CreateOrderModel>(nameof(Order)) // Sửa CreateOrderCommand thành OrderModel
					.Property("Total")
					.Message(MessageType.Valid)
					.Negative()
					.Build());

			//RuleFor(x => x.PaymentAmount)
			//	.Must((model, paymentAmount) => IsPaymentAmountValid(model, paymentAmount)) // Sửa tham số thành OrderModel
			//	.When(x => x.PaymentAmount > 0)
			//	.WithState(x => Messager
			//		.Create<CreateOrderModel>(nameof(Order)) // Sửa CreateOrderCommand thành OrderModel
			//		.Property(x => x.PaymentAmount)
			//		.Message(MessageType.LessThanEqual)
			//		.Negative()
			//		.Build());
		}

		private async Task<bool> IsTotalValidAsync(CreateOrderModel model, CancellationToken ct)
		{
			decimal amount = model.OrderItems?.Sum(i => i.Price) ?? 0m; 
			decimal discountValue = model.DiscountValue ?? 0m; 
			decimal total;

			if (model.DiscountType == null)
			{
				total = amount; // Không áp dụng giảm giá nếu DiscountType null
			}
			else
			{
				total = model.DiscountType.Value
					? amount * (1 - discountValue / 100) // Giảm theo phần trăm
					: amount - discountValue; // Giảm theo số tiền cố định
			}

			return total >= 0;
		}

		//private bool IsPaymentAmountValid(CreateOrderModel model, decimal paymentAmount)
		//{
		//	decimal amount = model.OrderItems?.Sum(i => i.Price) ?? 0m;
		//	decimal discountValue = model.DiscountValue ?? 0m; 
		//	decimal total;

		//	if (model.DiscountType == null)
		//	{
		//		total = amount; 
		//	}
		//	else
		//	{
		//		total = model.DiscountType.Value
		//			? amount * (1 - discountValue / 100) // Giảm theo phần trăm
		//			: amount - discountValue; // Giảm theo số tiền cố định
		//	}

		//	return paymentAmount <= total; // Số tiền thanh toán không vượt quá tổng
		//}
	}
}
