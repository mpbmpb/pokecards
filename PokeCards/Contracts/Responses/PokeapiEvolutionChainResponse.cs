using System.Text.Json.Serialization;
using PokeCards.Data;

namespace PokeCards.Contracts.Responses;

public class PokeapiEvolutionChainResponse
{
    [JsonPropertyName("id")] public int? Id { get; set; }
    [JsonPropertyName("chain")] public EvolutionChain? Chain { get; set; }
    
}