using System.Text.Json.Serialization;

namespace PokeCards.Contracts.Responses;

public class PokemontcgResponse
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("supertype")] public string Supertype { get; set; } // pokemon energy or trainer
    [JsonPropertyName("types")] public string[] Types { get; set; }
    [JsonPropertyName("images")]public PokemontcgImages Images { get; set; }
    
}

public record PokemontcgImages(string small, string large);