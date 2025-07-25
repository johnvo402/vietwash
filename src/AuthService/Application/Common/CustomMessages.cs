using Contracts.Common.Messages;

namespace Application.Common
{
    public static class CustomMessages
    {
        public static readonly CustomMessage FileSizeTooLarge = new(
            Message: "file-size-error",
            CustomMessageTranslations: new Dictionary<string, string>
            {
                { LanguageType.En.ToString(), "File size must be less than or equal to 50MB." },
                { LanguageType.Vi.ToString(), "Kích thước tệp phải nhỏ hơn hoặc bằng 50MB." },
            },
            NegativeMessage: "file-size-too-large"
        );

        public static readonly CustomMessage TotalFileSizeTooLarge = new(
            Message: "total-file-size-error",
            CustomMessageTranslations: new Dictionary<string, string>
            {
                { LanguageType.En.ToString(), "File size must be less than or equal to 200MB." },
                { LanguageType.Vi.ToString(), "Kích thước tệp phải nhỏ hơn hoặc bằng 200MB." },
            },
            NegativeMessage: "total-file-size-too-large"
        );

        public static readonly CustomMessage FileTypeInValid = new(
            Message: "file-type-error",
            CustomMessageTranslations: new Dictionary<string, string>
            {
                {
                    LanguageType.En.ToString(),
                    "Only supports jpg, png, webp, .bmp, .gif, .jpeg image formats."
                },
                {
                    LanguageType.Vi.ToString(),
                    "Chỉ hỗ trợ định dạng ảnh jpg, png, webp, .bmp, .gif, .jpeg."
                },
            },
            NegativeMessage: "file-type-invalid"
        );
    }
}
