namespace PokeCards.Data;

public class PokemonApiResponse
{
    public int Count { get; set; }
    public string? Next { get; set; }
    public string? Previous { get; set; }
    public List<SpeciesResponse> Results { get; set; } = new();

}

public record SpeciesResponse(string name, string url);