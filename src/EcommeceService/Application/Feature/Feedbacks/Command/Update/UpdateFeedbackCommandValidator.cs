using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Feedbacks;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Feedbacks.Command.Update
{
	public class UpdateFeedbackCommandValidator : AbstractValidator<UpdateFeedbackCommand>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IActionAccessorService _accessorService;

		public UpdateFeedbackCommandValidator(
			IUnitOfWork unitOfWork,
			IActionAccessorService accessorService
		)
		{
			_unitOfWork = unitOfWork;
			_accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			RuleFor(x => x.Feedback)
				.SetValidator(new FeedbackValidator(_unitOfWork, _accessorService));
			RuleFor(x => x.FeedbackId)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<UpdateFeedbackCommand>(nameof(Feedback))
						.Property(x => x.FeedbackId)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				)
				.MustAsync(IsFeedbackExistsAsync)
				.WithState(x =>
						Messager
							.Create<Feedback>()
							.Property(x => x.Id)
							.Message(MessageType.Found)
							.Negative()
							.Build()
				);
			RuleFor(x => x)
				.MustAsync((command, cancellation) =>
					IsEditableAsync(command.FeedbackId, command.Feedback.CustomerId, cancellation))
				.WithState(x =>
						Messager
							.Create<Feedback>()
							.Property(x => x.Id)
							.Message(MessageType.Expired)
							.Negative()
							.Build()
				);
		}
		private async Task<bool> IsFeedbackExistsAsync(long feedbackId, CancellationToken cancellation)
		{
			return await _unitOfWork.Repository<Feedback>().AnyAsync(x => x.Id == feedbackId && !x.Disable, cancellation);
		}
		private async Task<bool> IsEditableAsync(long feedbackId, long customerId, CancellationToken cancellationToken)
		{
			var feedback = await _unitOfWork.Repository<Feedback>()
				.QueryAsync()
				.Where(x => x.Id == feedbackId &&
							!x.Disable &&
							x.CustomerId == customerId)
				.Select(x => new { x.CreatedAt, HasReply = x.Replies.Any() })
				.FirstOrDefaultAsync(cancellationToken);

			if (feedback == null) return false;

			var withinEditTime = (DateTimeOffset.UtcNow - feedback.CreatedAt).TotalHours <= 24;
			return !feedback.HasReply && withinEditTime;
		}

	}
}
