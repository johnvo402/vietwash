using Application.Features.Common.Projections.Accounts;
using Mediator;

namespace Application.Features.Accounts.Commands.Profiles;

public class UpdateAccountProfileCommand : AccountModel, IRequest<UpdateAccountProfileResponse>;
