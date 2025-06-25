

namespace Contracts.Domain.BlackListToken
{
    public class BlackListToken
    {
        public string Token { get; set; } = default!;
        public DateTime ExpiredAt { get; set; }
    }
}
