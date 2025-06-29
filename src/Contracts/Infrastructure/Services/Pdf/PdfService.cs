using Contracts.Application.Common.Interfaces.Services.Pdf;
using Infrastructure.Services.Mail;
using PuppeteerSharp;

namespace Contracts.Infrastructure.Services.Pdf
{
    public class PdfService : IPdfService
    {
        private readonly RazorViewToStringRenderer _razorView;

        public PdfService(RazorViewToStringRenderer razorView)
        {
            _razorView = razorView;
        }

        public async Task<byte[]> GeneratePdfAsync(PdfGlobalParams settings)
        {
            string template = await _razorView.RenderViewToStringAsync(settings.Template!);
            await new BrowserFetcher().DownloadAsync();
            var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" },
                }
            );
            using var page = await browser.NewPageAsync();

            await page.SetContentAsync(template);
            var pdfBytes = await page.PdfDataAsync(
                new PdfOptions { Format = settings.PaperFormat, PrintBackground = true }
            );

            return pdfBytes;
        }
    }
}
