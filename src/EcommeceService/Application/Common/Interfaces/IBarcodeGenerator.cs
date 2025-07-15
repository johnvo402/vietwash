namespace Application.Common.Interfaces
{
    public interface IBarcodeGenerator
    {
        string GenerateBarcodeBase64(string content, int width = 300, int height = 100);
    }
}
