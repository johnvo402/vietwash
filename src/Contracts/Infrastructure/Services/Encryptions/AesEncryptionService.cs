using System.Security.Cryptography;
using System.Text;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Microsoft.Extensions.Options;

namespace Contracts.Infrastructure.Services.Encryptions
{
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public AesEncryptionService(IOptions<EncryptionOptions> options)
        {
            var opt = options.Value;

            if (opt.Key.Length != 32 || opt.IV.Length != 16)
                throw new ArgumentException("Key must be 32 chars, IV must be 16 chars");

            _key = Encoding.UTF8.GetBytes(opt.Key);
            _iv = Encoding.UTF8.GetBytes(opt.IV);
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}
