using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Bogus;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using PokeCards.Contracts.Responses;
using PokeCards.Data;
using PokeCards.Services;
using PokeCards.Tests.Helpers;
using Xunit.Abstractions;

namespace PokeCards.Tests.Unit;

//[Collection("PokemontcgService Unit tests")]
public class PokemontcgServiceTests
{
    private readonly IPokeapiService _pokeapiService;
    private static IPokeapiService _getPokeapiServiceWithId(int pokedexNumber)
    {
        var pokeapiService = Substitute.For<IPokeapiService>();
        pokeapiService.GetAllPokemonAsync().Returns(new List<Pokemon>{new Pokemon(pokedexNumber, "testPokemon")});
        return pokeapiService;
    }

    public PokemontcgServiceTests()
    {
        var pokeapiService = Substitute.For<IPokeapiService>();
        pokeapiService.GetAllPokemonAsync().Returns(new List<Pokemon>());
        _pokeapiService = pokeapiService;
    }

    [Fact]
    public async Task MockClientFactory_ShouldProduce_GivenResponse()
    {
        var guid = Guid.NewGuid();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://google.com"));
        var factory = new MockHttpHelper( new [] {guid.ToString()}).GetFactory();
        using var client = factory.CreateClient();

        using var response = await client.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        result.Should().Match(guid.ToString());
    }

    [Theory]
    [InlineData(3)]
    [InlineData(8)]
    [InlineData(42)]
    [InlineData(90)]
    public async Task GetCardsForAsync_ShouldReturn_SameNumberOfSeededCards(int numberOfCards)
    {
        var pokedexNumber = numberOfCards; // get a unique id for the test
        var responses = DataHelper.GetPokemontcgResponsesJson(pokedexNumber, numberOfCards, 35, 4);
        var factory = new MockHttpHelper(responses).GetFactory();
        var sut = new PokemontcgService(factory, _getPokeapiServiceWithId(pokedexNumber));
        
        var cards = await sut.GetAllCardsForAsync(1);
        
        cards.Count.Should().Be(numberOfCards);
    }

    [Fact]
    public async Task GetCardsForAsync_ShouldReturnCards_WithSimilarProperties()
    {
        var responses = DataHelper.GetPokemontcgResponsesJson(10, 35, 4);
        var factory = new MockHttpHelper(responses).GetFactory();
        var sourceCards = DataHelper.ExtractCards(responses);
        var sut = new PokemontcgService(factory, _pokeapiService);

        var cards = await sut.GetAllCardsForAsync(1);

        cards.Should().BeEquivalentTo(sourceCards);
    }

    [Fact]
    public async Task CachedEntry_ShouldNotMakeNewCallToApi()
    {
        var response1 = DataHelper.GetPokemontcgResponsesJson(1,10, 35, 4);
        var response2 = DataHelper.GetPokemontcgResponsesJson(1,20, 35, 4);
        var responses = response1.Concat(response2).ToArray();
        
        var factory = new MockHttpHelper(responses).GetFactory();
        var sut = new PokemontcgService(factory, _pokeapiService);

        var cards = await sut.GetAllCardsForAsync(1);
        var cards2 = await sut.GetAllCardsForAsync(1);

        cards.Should().BeEquivalentTo(cards2);
        cards.Count.Should().Be(10);
    }

    [Fact]
    public async Task CachedEntry_ShouldBeEvicted_WhenSizeLimitIsExceeded()
    {
        var response1 = DataHelper.GetPokemontcgResponsesJson(1,10, 35, 4);
        var response2 = DataHelper.GetPokemontcgResponsesJson(2,20, 35, 4);
        var response3 = DataHelper.GetPokemontcgResponsesJson(1,1, 35, 4);
        var responses = response1.Concat(response2).Concat(response3).ToArray();
        
        var factory = new MockHttpHelper(responses).GetFactory();
        var sut = new PokemontcgService(factory, _pokeapiService, 1);

        await sut.GetAllCardsForAsync(1);
        await sut.GetAllCardsForAsync(2);
        var cards = await sut.GetAllCardsForAsync(1);

        cards.Count.Should().Be(1);
    }

    [Fact]
    public async Task NewEntry_ShouldBeStore_WhenOldOneIsEvicted()
    {
        var response1 = DataHelper.GetPokemontcgResponsesJson(1,10, 35, 4);
        var response2 = DataHelper.GetPokemontcgResponsesJson(2,20, 35, 4);
        var response3 = DataHelper.GetPokemontcgResponsesJson(3,3, 35, 4);
        var response4 = DataHelper.GetPokemontcgResponsesJson(3,2, 35, 4);
        var response5 = DataHelper.GetPokemontcgResponsesJson(3,1, 35, 4);
        var responses = response1.Concat(response2).Concat(response3).Concat(response4).Concat(response5).ToArray();
        
        var factory = new MockHttpHelper(responses).GetFactory();
        var sut = new PokemontcgService(factory, _pokeapiService, 2);

        await sut.GetAllCardsForAsync(1);
        await sut.GetAllCardsForAsync(2);
        await sut.GetAllCardsForAsync(3);
        var cards3 = await sut.GetAllCardsForAsync(3);
        var cards2 = await sut.GetAllCardsForAsync(2);

        cards3.Count.Should().Be(3);
        cards2.Count.Should().Be(20);
    }
    
}