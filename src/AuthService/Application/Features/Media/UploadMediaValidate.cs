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
        private const long MaxTotalSize = 200 * 1024 * 1024; // 200MB

        public UploadMediaValidate()
        {
            RuleFor(x => x.File)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<UploadMediaCommand>()
                        .Property(x => x.File!)
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
                                .Property(d => d.File)
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
                                .Property(d => d.File)
                                .Message(CustomMessages.FileTypeInValid)
                                .Negative()
                                .Build()
                        );
                });
            RuleFor(x => x.Files)
                .Must(files => files.Sum(f => f.Length) <= MaxTotalSize)
                .WithState(x =>
                    Messager
                        .Create<UploadMediaCommand>()
                        .Property(d => d.Files)
                        .Message(CustomMessages.TotalFileSizeTooLarge)
                        .Negative()
                        .Build()
                );
        }
    }
}
