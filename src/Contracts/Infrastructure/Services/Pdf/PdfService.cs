using Contracts.Application.Common.Interfaces.Services.Pdf;
using DinkToPdf;
using DinkToPdf.Contracts;
using Infrastructure.Services.Mail;

namespace Contracts.Infrastructure.Services.Pdf
{
    public class PdfService : IPdfService
    {
        private readonly IConverter _converter;
        private readonly RazorViewToStringRenderer _razorView;

        public PdfService(IConverter converter, RazorViewToStringRenderer razorView)
        {
            _converter = converter;
            _razorView = razorView;
        }

        public async Task<byte[]> GeneratePdfAsync(PdfGlobalParams settings)
        {
            string template = await _razorView.RenderViewToStringAsync(settings.Template!);
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings =
                {
                    PaperSize = settings.PaperSize,
                    Orientation = settings.Orientation,
                    DocumentTitle = settings.DocumentTitle,
                    Margins = new MarginSettings
                    {
                        Top = settings.MarginTop,
                        Bottom = settings.MarginBottom,
                        Left = settings.MarginLeft,
                        Right = settings.MarginRight,
                    },
                },
                Objects =
                {
                    new ObjectSettings()
                    {
                        HtmlContent = template,
                        WebSettings = { DefaultEncoding = "utf-8" },
                    },
                },
            };

            return _converter.Convert(doc);
        }
    }
}
