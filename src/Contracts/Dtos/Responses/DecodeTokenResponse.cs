using System.Text.Json.Serialization;

namespace Contracts.Dtos.Responses;

public class DecodeTokenResponse
{
    [JsonPropertyName("sub")]
    public string? Sub { get; set; }

    [JsonPropertyName("family_id")]
    public string? FamilyId { get; set; }
    [JsonPropertyName("cnf")]
    public TokenCnfModel? Cnf { get; set; }
    [JsonPropertyName("exp")]
    public long? ExpiredTime { get; set; }
}

public class TokenCnfModel
{
    [JsonPropertyName("jwk")]
    public JwkModel? Jwk { get; set; }
}

public class JwkModel
{
    [JsonPropertyName("kty")]
    public string Kty { get; set; } = "EC";
    [JsonPropertyName("crv")]
    public string Crv { get; set; } = "P-256";
    [JsonPropertyName("x")]
    public string? X { get; set; }
    [JsonPropertyName("y")]
    public string? Y { get; set; }
}