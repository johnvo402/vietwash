using Application.Common.Interfaces.Services.DistributedCache;
using Contracts.Application.Common.Interfaces.Services.Token;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Infrastructure.Services.Token;
using JohnChum.SharedKernel.Extensions;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;


namespace Contracts.Infrastructure.Services.Token
{
    public class TokenSecurityService(IOptions<JwtSettings> jwtSettings, IRedisCacheService _cache) : ITokenSecurityService
    {
        private readonly JwtSettings settings = jwtSettings.Value;
        private const string BlacklistPrefix = "blacklist_";

        public async Task AddToBlacklistAsync(string token, TimeSpan expiry)
        {
            string key = $"{BlacklistPrefix}{token}";
            await _cache.Database.StringSetAsync(key, "blacklisted", expiry);
        }
        public DecodeTokenResponse DecodeToken(string token)
        {
            var json = JwtBuilder
                .Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(settings.SecretKey).Issuer(settings.Issuer).Audience(settings.Audience)
                .MustVerifySignature()
                .Decode(token);

            return SerializerExtension.Deserialize<DecodeTokenResponse>(json).Object!;
        }

        public async Task<bool> VerifySignatureAsync(TokenBinding request)
        {
            var decodeToken = DecodeToken(request.Token!);
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Kiểm tra timestamp có quá cũ không
            if (Math.Abs(now - request.Timestamp) > 100 * 1000)
                return false;
            if (await ExistsNonceAsync(request.Nonce!)) return false;
            await StoreNonceAsync(request.Nonce!);

            var data = $"{request.Timestamp}:{request.Nonce}";
            return VerifySignature(decodeToken?.PublicKey!, data, request.Signature!);
        }

        public async Task<bool> ExistsNonceAsync(string nonce) =>
            (await _cache.Database.StringGetAsync(nonce)).HasValue;

        public async Task StoreNonceAsync(string nonce) =>
            await _cache.Database.StringSetAsync(nonce, "Nonce", TimeSpan.FromMinutes(5));
        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            string key = $"{BlacklistPrefix}{token}";
            return await _cache.Database.KeyExistsAsync(key);
        }

        public bool VerifySignature(string publicKeyHex, string message, string signatureHex)
        {
            try
            {
                // Chuyển đổi public key từ hex sang dạng byte
                byte[] publicKeyBytes = Convert.FromHexString(publicKeyHex);
                byte[] signatureBytes = Convert.FromHexString(signatureHex);
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                using (ECDsa ecdsa = ECDsa.Create())
                {
                    ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
                    return ecdsa.VerifyData(messageBytes, signatureBytes, HashAlgorithmName.SHA256);
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task AddSessionUserAsync(Ulid userId, string userData, TimeSpan expiry)
        {

            await _cache.Database.StringSetAsync(
                userId.ToString(),
                userData,
                expiry,
                when: When.Always
            );
        }
    }
}
