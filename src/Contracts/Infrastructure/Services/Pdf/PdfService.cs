using Contracts.Application.Common.Interfaces.Services.Pdf;
using Infrastructure.Services.Mail;
using PuppeteerSharp;

namespace Contracts.Infrastructure.Services.Pdf
{
    public class PdfService : IPdfService, IAsyncDisposable
    {
        private readonly RazorViewToStringRenderer _razorView;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private IBrowser _browser;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public PdfService(RazorViewToStringRenderer razorView)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            _razorView = razorView;
        }

        private async Task<IBrowser> GetOrCreateBrowserAsync()
        {
            if (_browser != null && _browser.IsConnected)
                return _browser;

            await _semaphore.WaitAsync();
            try
            {
                if (_browser == null || !_browser.IsConnected)
                {
                    await new BrowserFetcher().DownloadAsync();
                    _browser = await Puppeteer.LaunchAsync(
                        new LaunchOptions
                        {
                            Headless = true,
                            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" },
                        }
                    );
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return _browser;
        }

        public async Task<byte[]> GeneratePdfAsync(PdfGlobalParams settings)
        {
            string template = await _razorView.RenderViewToStringAsync(settings.Template!);

            var browser = await GetOrCreateBrowserAsync();

            await using var page = await browser.NewPageAsync();


            await page.SetContentAsync(template);

            var pdfBytes = await page.PdfDataAsync(
                new PdfOptions { Format = settings.PaperFormat, PrintBackground = true }
            );

            return pdfBytes;
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser.Dispose();
                _browser = null;
            }
            _semaphore.Dispose();
        }
    }
}
