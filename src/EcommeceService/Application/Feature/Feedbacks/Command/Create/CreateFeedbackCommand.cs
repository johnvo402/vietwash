using Application.Feature.Common.Projections.Feedbacks;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Feedbacks.Command.Create;

public class CreateFeedbackCommand : FeedbackModel, IRequest<Result>;
