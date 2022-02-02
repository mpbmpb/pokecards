using System.Text.Json.Serialization;

namespace PokeCards.Contracts.Responses;

public class PokeapiEvolutionChainResponse
{
    [JsonPropertyName("id")] public int? Id { get; set; }
    [JsonPropertyName("chain")] public EvolutionChain? Chain { get; set; }
    
}


public class EvolutionChain
{
    [JsonPropertyName("species")] public SpeciesResponse Species { get; set; }
    [JsonPropertyName("evolves_to")] public List<EvolutionChain?>? EvolvesTo { get; set; }
}