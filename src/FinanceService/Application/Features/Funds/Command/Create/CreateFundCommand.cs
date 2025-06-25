using Application.Features.Common.Projections.Funds;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Funds.Command.Create;

public class CreateFundCommand : CreateFundModel, IRequest<Result>;
