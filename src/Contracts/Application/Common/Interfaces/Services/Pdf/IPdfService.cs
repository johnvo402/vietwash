namespace Contracts.Application.Common.Interfaces.Services.Pdf
{
    public interface IPdfService
    {
        Task<byte[]> GeneratePdfAsync(PdfGlobalParams param);
    }
}
