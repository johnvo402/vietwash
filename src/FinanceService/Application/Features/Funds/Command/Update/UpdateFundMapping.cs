using Application.Features.Common.Projections.Funds;
using Domain.Aggregates.Funds;

namespace Application.Features.Funds.Command.Update
{
    public static class UpdateFundMapping
    {
        public static void MapUpdateToEntity(this UpdateFundModel command, Fund fund)
        {
            fund.Update(
                note: command.Note,
                status: command.Status,
                paymentMethod: command.PaymentMethod
            );
        }
    }
}
