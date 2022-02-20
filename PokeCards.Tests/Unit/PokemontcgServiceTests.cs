using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NSubstitute;
using PokeCards.Services;
using PokeCards.Tests.Helpers;

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

}