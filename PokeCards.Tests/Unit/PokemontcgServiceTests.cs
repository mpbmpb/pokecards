using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
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
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://google.com"));
        var factory = MockHttpHelper.GetFactory(responses);

        var pokeapiService = Substitute.For<IPokeapiService>();
        pokeapiService.GetAllPokemonAsync().Returns(new List<Pokemon>());
        var sut = new PokemontcgService(factory, pokeapiService);

        var cards = await sut.GetAllCardsForAsync(1);
        
        cards.Count.Should().Be(numberOfCards);
    }

    public void Dispose()
    {
        GC.Collect();
    }
}