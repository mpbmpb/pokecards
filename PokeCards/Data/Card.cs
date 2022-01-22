using PokeCards.Contracts.Responses;

namespace PokeCards.Data;

public class Card
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Supertype { get; set; } = "";
    public string[] Types { get; set; } = Array.Empty<string>();
    public string ImageUrl { get; set; } = "";
}