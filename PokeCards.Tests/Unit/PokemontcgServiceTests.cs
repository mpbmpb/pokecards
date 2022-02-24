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

[Collection("PokemontcgService Unit tests")]
public class PokemontcgServiceTests : IDisposable
{
    private readonly IPokeapiService _pokeapiService;

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
        var factory = MockHttpHelper.GetFactory(guid.ToString());
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
        var responses = DataHelper.GetPokemontcgResponsesJson(numberOfCards, 35, 4);
        var factory = MockHttpHelper.GetFactory(responses);
        var sut = new PokemontcgService(factory, _pokeapiService);
        
        var cards = await sut.GetAllCardsForAsync(1);
        
        cards.Count.Should().Be(numberOfCards);
    }

    [Fact]
    public async Task GetCardsForAsync_ShouldReturnCards_WithSimilarProperties()
    {
        var responses = DataHelper.GetPokemontcgResponsesJson(10, 35, 4);
        var factory = MockHttpHelper.GetFactory(responses);
        var sourceCards = DataHelper.ExtractCards(responses);
        var sut = new PokemontcgService(factory, _pokeapiService);

        var cards = await sut.GetAllCardsForAsync(1);

        cards.Should().BeEquivalentTo(sourceCards);
    }



    public void Dispose()
    {
        GC.Collect();
    }
}