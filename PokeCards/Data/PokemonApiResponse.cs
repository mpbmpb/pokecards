namespace PokeCards.Data;

public class PokemonApiResponse
{
    public int Count { get; set; }
    public string? Next { get; set; }
    public string? Previous { get; set; }
    public List<PokemonSpecies> Results { get; set; } = new();

}

public record PokemonSpecies(string name, string url);