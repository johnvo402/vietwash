namespace Contracts.Infrastructure.Services.Encryptions
{
    public class EncryptionOptions
    {
        public string Key { get; set; } = null!;
        public string IV { get; set; } = null!;
    }
}
