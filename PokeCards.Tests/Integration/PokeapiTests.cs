using System.Threading.Tasks;
using PokeCards.Services;
using PokeCards.Tests.InfraStructure;

// ReSharper disable StringLiteralTypo

namespace PokeCards.Tests.Integration;

public class PokeapiTests : IntegrationTestBase
{
    private readonly PokeapiService _sut;

    public PokeapiTests()
    {
        _sut = new PokeapiService(ClientFactory);
    }
    
    [Fact]
    public async Task GetAllPokemonAsync_gets_898_pokemon_species()
    {
        var result = await _sut.GetAllPokemonAsync();

        result.Count.Should().Be(898);
    }

    [Fact]
    public async Task GetAllPokemonAsync_should_get_all_species()
    {
        var result = await _sut.GetAllPokemonAsync();

        result[0].Name.Should().Match("bulbasaur");
        result[0].Id.Should().Be(1);
        result[897].Name.Should().Match("calyrex");
        result[897].Id.Should().Be(898);
    }

    [Theory]
    [InlineData(1, "bulbasaur")]
    [InlineData(6, "charizard")]
    [InlineData(25, "pikachu")]
    [InlineData(150, "mewtwo")]
    [InlineData(319, "sharpedo")]
    [InlineData(505, "watchog")]
    [InlineData(758, "salazzle")]
    [InlineData(888, "zacian")]
    public async Task GetPokemonAsync_gets_correct_pokemon(int id, string name)
    {
        var result = await _sut.GetPokemonAsync(id);

        result.Name.Should().Match(name);
        result.Id.Should().Be(id);
    }

    [Theory]
    [InlineData(1, 0, 151)]
    [InlineData(3, 251, 135)]
    [InlineData(7, 721, 88)]
    public void GenerationRange_gives_correct_offset_and_count_for_given_generation(int gen, int offset, int count)
    {
        var (genOffset, populationCount) = _sut.GenerationRange(gen);

        genOffset.Should().Be(offset);
        populationCount.Should().Be(count);
    }

    [Theory]
    [InlineData(1, "bulbasaur", "mew")]
    [InlineData(2, "chikorita", "celebi")]
    [InlineData(3, "treecko", "deoxys")]
    [InlineData(4, "turtwig", "arceus")]
    [InlineData(5, "victini", "genesect")]
    [InlineData(6, "chespin", "volcanion")]
    [InlineData(7, "rowlet", "melmetal")]
    [InlineData(8, "grookey", "calyrex")]
    public async Task GenerationRange_points_to_first_and_last_species_in_generation(int gen, string firstPokemon, string lastPokemon)
    {
        var pokemon = await _sut.GetAllPokemonAsync();
        var genRange = _sut.GenerationRange(gen);

        var result = pokemon.GetRangeOrLess(genRange.offset, genRange.populationCount);

        result.First().Name.Should().Match(firstPokemon);
        result.Last().Name.Should().Match(lastPokemon);
    }
}