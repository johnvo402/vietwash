using FluentValidation;
using Contracts.Common.Messages;
using Microsoft.AspNetCore.Http;

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

        public UploadMediaValidate()
        {
            RuleFor(x => x.File)
                .NotNull()
                .WithState(x =>
                    Messager
                        .Create<UploadMediaCommand>()
                        .Property(x => x.File!)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.File)
                .Must((request, file) => IsValidFileType(file))
                .WithState(x =>
                    Messager
                        .Create<UploadMediaCommand>()
                        .Property(x => x.File!)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build()
                );
        }

        private bool IsValidFileType(IFormFile file)
        {
            if (file == null)
                return false;
            var extension = Path.GetExtension(file.FileName)?.ToLower();

            return ImageExtensions.Contains(extension);
        }
    }
}
