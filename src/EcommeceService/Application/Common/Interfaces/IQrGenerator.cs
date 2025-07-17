namespace Application.Common.Interfaces
{
    public interface IQrGenerator
    {
        string GenerateQrBase64(string content);
        byte[] GenerateQrCode(string text);
    }
}
