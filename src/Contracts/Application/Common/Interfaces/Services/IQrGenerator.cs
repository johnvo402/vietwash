namespace Application.Common.Interfaces.Services
{
    public interface IQrGenerator
    {
        string GenerateQrBase64(string content);
        byte[] GenerateQrCode(string text);
    }
}
