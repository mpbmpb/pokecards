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

public class PokemontcgServiceTests
{
    
    [Fact]
    public async Task MockClientFactory_ShouldProduce_GivenResponse()
    {
        var guid = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://google.com"));
        var factory = MockHttpHelper.GetFactory(guid.ToString());
        var client = factory.CreateClient();

        var response = await client.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        result.Should().Match(guid.ToString());
    }

    [Fact]
    public async Task GetCardsForAsync_ShouldReturn_AllSeededCards()
    {
        var responses = DataHelper.GetPokemontcgResponsesJson(3, 35, 4);
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://google.com"));
        var factory = MockHttpHelper.GetFactory(responses);

        var pokeapiService = new PokeapiService(factory);
        var sut = new PokemontcgService(factory, pokeapiService);

        var cards = await sut.GetAllCardsForAsync(1);

    }

}