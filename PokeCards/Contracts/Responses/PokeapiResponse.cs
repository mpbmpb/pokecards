using System.Text.Json.Serialization;

namespace PokeCards.Contracts.Responses;

public class PokeapiResponse
{
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("next")] public string? Next { get; set; }
    [JsonPropertyName("previous")] public string? Previous { get; set; }
    [JsonPropertyName("results")] public List<SpeciesResponse> Results { get; set; } = new();

}

public record SpeciesResponse(string name, string url);