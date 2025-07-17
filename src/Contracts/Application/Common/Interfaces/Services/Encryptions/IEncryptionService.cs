namespace Contracts.Application.Common.Interfaces.Services.Encryptions
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
