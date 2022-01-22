using System.Text.Json.Serialization;

namespace PokeCards.Contracts.Responses;

public class PokemontcgResponse
{
    [JsonPropertyName("data")] public PokemontcgCardResponse[]? Data { get; set; }
    [JsonPropertyName("page")] public int? Page { get; set; }
    [JsonPropertyName("pageSize")] public int? PageSize { get; set; }
    [JsonPropertyName("count")] public int? Count { get; set; }
    [JsonPropertyName("totalCount")] public int? TotalCount { get; set; }
}