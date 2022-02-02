using System.Text.Json.Serialization;

namespace PokeCards.Contracts.Responses;

public class PokeapiSpeciesDetailResponse
{
    [JsonPropertyName("id")] public int? Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("evolution_chain")] public EvolutionChainShort? EvolutionChain { get; set; }
    // [JsonPropertyName("evolves_from_species")] public SpeciesResponse? EvolvesFromSpecies { get; set; }

}

public record EvolutionChainShort(string url);

