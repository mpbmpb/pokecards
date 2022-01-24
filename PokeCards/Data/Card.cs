using PokeCards.Contracts.Responses;
using PokeCards.Pages;

namespace PokeCards.Data;

public class Card
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Supertype { get; set; } = "";
    public string[] Types { get; set; } = Array.Empty<string>();

    public List<Pokemon> Pokemons { get; set; } = new();
    public string ImageUrl { get; set; } = "";
}