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
            var decodeToken = DecodeToken(request.Token);

            if (await ExistsNonceAsync(request.Nonce)) return false;
            await StoreNonceAsync(request.Nonce);

            var data = $"{request.Timestamp}:{request.Nonce}";
            return VerifySignature(decodeToken?.Cnf?.Jwk!, data, request.Signature);
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

        private static bool VerifySignature(JwkModel jwk, string data, string base64Signature)
        {
            try
            {
                byte[] xBytes = Convert.FromBase64String(jwk.X!);
                byte[] yBytes = Convert.FromBase64String(jwk.Y!);

                var ecParams = new ECParameters
                {
                    Q = new ECPoint
                    {
                        X = xBytes,
                        Y = yBytes
                    },
                    Curve = ECCurve.NamedCurves.nistP256
                };

                using var ecdsa = ECDsa.Create(ecParams);

                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] signatureBytes = Convert.FromBase64String(base64Signature);

                return ecdsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256);
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
