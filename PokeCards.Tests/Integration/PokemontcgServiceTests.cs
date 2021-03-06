using System.Threading.Tasks;
using PokeCards.Services;
using PokeCards.Tests.InfraStructure;

namespace PokeCards.Tests.Integration;

//[Collection("PokemontcgService Integration tests")]
public class PokemontcgServiceTests : IClassFixture<ServicesIntegrationTestBase>
{
    private readonly PokemontcgService _sut;

    public PokemontcgServiceTests(ServicesIntegrationTestBase testBase)
    {
        _sut = testBase.PokemontcgService;
    }

    [Theory]
    [InlineData(6, 78, "charizard" )]   // current number of cards, will increase as cards are added
    [InlineData(7, 22, "squirtle" )]
    [InlineData(150, 66, "mewtwo" )]
    [InlineData(722, 16, "rowlet" )]
    public async Task GetAllCardsForAsync_gets_correct_number_of_cards_for_that_pokemon(int id, int count, string name)
    {
        var result = await _sut.GetAllCardsForAsync(id);
        result.Count.Should().BeGreaterOrEqualTo(count);                           // cards get added but never removed from db
        result.Count(card => card.Pokemons.Any(p => p.Name == name)).Should().BeGreaterOrEqualTo(count);
    }

    [Theory]
    [InlineData(722, "sma-SV2", "Rowlet", "Pokémon", new string[]{"Grass"}, "https://images.pokemontcg.io/sma/SV2.png")]
    [InlineData(722, "sm11-215", "Rowlet & Alolan Exeggutor-GX", "Pokémon", new string[]{"Grass"}, "https://images.pokemontcg.io/sm11/215.png")]
    [InlineData(443, "pop6-7", "Gible", "Pokémon", new string[]{"Colorless"}, "https://images.pokemontcg.io/pop6/7.png")]
    [InlineData(443, "bw6-86", "Gible", "Pokémon", new string[]{"Dragon"}, "https://images.pokemontcg.io/bw6/86.png")]
    public async Task GetAllCardsForAsync_gets_correct_cards(int pokedexNo, string id, string name, string supertype, string[] types, string imageUrl)
    {
        var cards = await _sut.GetAllCardsForAsync(pokedexNo);

        var result = cards.First(c => c.Id == id);

        result.Name.Should().Match(name);
        result.Supertype.Should().Match(supertype);
        result.Types.Should().Equal(types);
        result.ImageUrl.Should().Match(imageUrl);
    }
}