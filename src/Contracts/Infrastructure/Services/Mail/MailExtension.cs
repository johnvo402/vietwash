using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Mail;
using Contracts.Application.Common.Interfaces.Services.Pdf;
using Contracts.Infrastructure.Services.Pdf;
using Infrastructure.Services.QrCodes;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Mail;

public static class MailExtension
{
    public static IServiceCollection AddMailPdf(this IServiceCollection services)
    {
        return services
            .AddTransient<IMailService, MailService>()
            .AddTransient<IPdfService, PdfService>()
            .AddSingleton<RazorViewToStringRenderer>()
            .AddSingleton<IQrGenerator, QrCodeGenerator>();
    }
}
