using System.Text.Json.Serialization;
using PokeCards.Contracts.Responses;

namespace PokeCards.Data;

public class EvolutionChain
{
    [JsonPropertyName("species")] public SpeciesResponse Species { get; set; }
    [JsonPropertyName("evolves_to")] public List<EvolutionChain?>? EvolvesTo { get; set; }
}