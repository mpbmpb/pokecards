// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace PokeCards.Data;

public class Card
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Supertype { get; set; } = "";
    public string[] Types { get; set; } = Array.Empty<string>();
    public List<Pokemon> Pokemons { get; set; } = new();
    public string ImageUrl { get; set; } = "";
    public string ImageType { get; set; } = "png";
    public byte[] ImageBytes { get; set; } = new byte[]{};
}