using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Feedbacks;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using FluentValidation;

namespace Application.Feature.Common.Validators.Feedbacks
{
	public class FeedbackValidator : AbstractValidator<FeedbackModel>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;

		public FeedbackValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
		{
			this.unitOfWork = unitOfWork;
			this.accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			RuleFor(x => x.Comment)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<FeedbackModel>(nameof(Feedback))
						.Property(x => x.Comment)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				)
				.MaximumLength(500)
				.WithState(x =>
					Messager
						.Create<FeedbackModel>(nameof(Feedback))
						.Property(x => x.Comment)
						.Message(MessageType.MaximumLength)
						.Build()
				);
			RuleFor(x => x.Rating)
				.GreaterThan(0)
				.WithState(x =>
					Messager
						.Create<FeedbackModel>(nameof(Feedback))
						.Property(x => x.Rating)
						.Message(MessageType.GreaterThan)
						.Negative()
						.Build()
				)
				.LessThanOrEqualTo(5)
				.WithState(x =>
					Messager
						.Create<FeedbackModel>(nameof(Feedback))
						.Property(x => x.Rating)
						.Message(MessageType.LessThanEqual)
						.Negative()
						.Build()
				);
			RuleFor(x => x.ServiceId)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<FeedbackModel>(nameof(Feedback))
						.Property(x => x.ServiceId)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				);
			RuleFor(x => x.CustomerId)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<FeedbackModel>(nameof(Feedback))
						.Property(x => x.CustomerId)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				);
			RuleFor(x => x)
				.MustAsync((model, cancellationToken) =>
					IsCustomerUseServiceAsync(model.CustomerId, model.ServiceId, cancellationToken))
				.WithState(x =>
					Messager
						.Create<FeedbackModel>(nameof(Feedback))
						.Property(x => x.CustomerId)
						.Message(MessageType.Existence)
						.Negative()
						.Build()
				);
		}

		private async Task<bool> IsCustomerUseServiceAsync(
			long customerId,
			long serviceId,
			CancellationToken cancellationToken
		)
		{
			return await unitOfWork.Repository<Order>()
					.AnyAsync(
						order => order.CustomerId == customerId
								 && order.OrderItems.Any(item => item.ServiceId == serviceId)
								 && order.Status == OrderStatus.Completed,
						cancellationToken
					);
		}
	}
}
