using Application.Common;
using Contracts.Common.Messages;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace Application.Features.Media
{
    public class UploadMediaValidate : AbstractValidator<UploadMediaCommand>
    {
        private static readonly string[] ImageExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".bmp",
            ".webp",
        };

        private const long MaxFileSize = 50 * 1024 * 1024; // 50MB

        public UploadMediaValidate()
        {
            RuleForEach(x => x.Files)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<UploadMediaCommand>()
                        .Property(x => x.Files!)
                        .Message(MessageType.Empty)
                        .Negative()
                        .Build()
                )
                .ChildRules(item =>
                {
                    item.RuleFor(x => x.Length)
                        .LessThanOrEqualTo(MaxFileSize)
                        .WithState(x =>
                            Messager
                                .Create<UploadMediaCommand>()
                                .Property(d => d.Files)
                                .Message(CustomMessages.FileSizeTooLarge)
                                .Negative()
                                .Build()
                        );
                    item.RuleFor(x => x.FileName)
                        .Must(fileName =>
                            ImageExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant())
                        )
                        .WithState(x =>
                            Messager
                                .Create<UploadMediaCommand>()
                                .Property(d => d.Files)
                                .Message(CustomMessages.FileTypeInValid)
                                .Negative()
                                .Build()
                        );
                });
        }
    }
}
