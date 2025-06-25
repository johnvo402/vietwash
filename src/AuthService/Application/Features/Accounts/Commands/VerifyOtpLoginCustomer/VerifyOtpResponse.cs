using Application.Features.Common.Projections.Accounts;

namespace Application.Features.Accounts.Commands.VerifyOtpLoginCustomer
{
    public class VerifyOtpResponse : AccountTokenProjection
    {
        public bool Verified { get; set; }
        public bool? IsNew { get; set; } = null;
    }
}
