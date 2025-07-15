using Application.Common.Interfaces;
using SkiaSharp;
using ZXing;
using ZXing.SkiaSharp.Rendering;

namespace Infrastructure.Services.Barcodes
{
    public class BarcodeGenerator : IBarcodeGenerator
    {
        public string GenerateBarcodeBase64(string content, int width = 300, int height = 100)
        {
            var writer = new BarcodeWriter<SKBitmap>
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 1,
                },
                Renderer = new SKBitmapRenderer(),
            };

            using var bitmap = writer.Write(content);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var bytes = data.ToArray();
            return Convert.ToBase64String(bytes);
        }
    }
}
