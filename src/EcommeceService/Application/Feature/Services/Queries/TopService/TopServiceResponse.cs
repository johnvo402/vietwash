using Application.Common.Security;

namespace Application.Feature.Services.Queries.TopService
{
    public class TopServiceResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        [File]
        public string? Image { get; set; }

        public decimal BasePrice { get; set; }

        public int TotalUsed { get; set; }
    }
}
