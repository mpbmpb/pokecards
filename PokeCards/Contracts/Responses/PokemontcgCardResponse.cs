using System.Text.Json.Serialization;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PokeCards.Contracts.Responses;

public class PokemontcgCardResponse
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("supertype")] public string? Supertype { get; set; } // pokemon energy or trainer
    [JsonPropertyName("types")] public string[]? Types { get; set; }
    
    [JsonPropertyName("nationalPokedexNumbers")] public int[]? PokedexNumbers { get; set; }
    [JsonPropertyName("images")]public PokemonCardImageUrls? Images { get; set; }
    
}

public record PokemonCardImageUrls(string? small, string? large);