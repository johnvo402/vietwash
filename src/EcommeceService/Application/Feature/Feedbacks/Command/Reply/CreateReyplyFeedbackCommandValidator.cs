using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using FluentValidation;

namespace Application.Feature.Feedbacks.Command.Reply
{
	public class CreateReyplyFeedbackCommandValidator : AbstractValidator<CreateReyplyFeedbackCommand>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IActionAccessorService _accessorService;

		public CreateReyplyFeedbackCommandValidator(
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
			RuleFor(x => x.Comment)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<CreateReyplyFeedbackCommand>(nameof(Feedback))
						.Property(x => x.Comment)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				)
				.MaximumLength(500)
				.WithState(x =>
					Messager
						.Create<CreateReyplyFeedbackCommand>(nameof(Feedback))
						.Property(x => x.Comment)
						.Message(MessageType.MaximumLength)
						.Build()
				);
			RuleFor(x => x.StaffId)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<CreateReyplyFeedbackCommand>(nameof(Feedback))
						.Property(x => x.StaffId)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				);
			RuleFor(x => x.ParentId)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<CreateReyplyFeedbackCommand>(nameof(Feedback))
						.Property(x => x.ParentId)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				)
				.MustAsync(IsFeedbackExistsAsync)
				.WithState(_ =>
					Messager
						.Create<CreateReyplyFeedbackCommand>(nameof(Feedback))
						.Property(x => x.ParentId)
						.Message(MessageType.Found)
						.Negative()
						.Build()
				);
		}
		private async Task<bool> IsFeedbackExistsAsync(long feedbackId, CancellationToken cancellation)
		{
			return await _unitOfWork
				.Repository<Feedback>()
				.AnyAsync(p => p.Id == feedbackId, cancellation);
		}
	}
}
