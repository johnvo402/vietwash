using Contracts.Dtos.Requests;
using DinkToPdf;

namespace Contracts.Application.Common.Interfaces.Services.Pdf
{
    public class PdfGlobalParams
    {
        public PaperKind PaperSize { get; set; } = PaperKind.A4;
        public Orientation Orientation { get; set; } = Orientation.Portrait;
        public int MarginTop { get; set; } = 10;
        public int MarginBottom { get; set; } = 10;
        public int MarginLeft { get; set; } = 10;
        public int MarginRight { get; set; } = 10;
        public string DocumentTitle { get; set; } = "PDF Document";
        public required MailTemplate? Template { get; set; }
    }
}
