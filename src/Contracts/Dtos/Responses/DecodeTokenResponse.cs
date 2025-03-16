using System.Text.Json.Serialization;

namespace Contracts.Dtos.Responses;

public class DecodeTokenResponse
{
    [JsonPropertyName("sub")]
    public string? Sub { get; set; }

    [JsonPropertyName("family_id")]
    public string? FamilyId { get; set; }
    [JsonPropertyName("public_key")]
    public string? PublicKey { get; set; }
    [JsonPropertyName("exp")]
    public long? ExpiredTime { get; set; }
}

