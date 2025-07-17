using Application.Common.Interfaces;
using QRCoder;

namespace Infrastructure.Services.QrCodes
{
    public class QrCodeGenerator : IQrGenerator
    {
        public string GenerateQrBase64(string text)
        {
            var bytes = GenerateQrCode(text);
            return "data:image/png;base64," + Convert.ToBase64String(bytes);
        }

        public byte[] GenerateQrCode(string text)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q)
            )
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(20);
            }
        }
    }
}
