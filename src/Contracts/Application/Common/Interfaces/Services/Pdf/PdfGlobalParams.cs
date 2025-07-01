using Contracts.Dtos.Requests;
using PuppeteerSharp.Media;

namespace Contracts.Application.Common.Interfaces.Services.Pdf
{
    public class PdfGlobalParams
    {
        public PaperFormat PaperFormat { get; set; } = PaperFormat.A4;
        public required MailTemplate? Template { get; set; }
    }
}
